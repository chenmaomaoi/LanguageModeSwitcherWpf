using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Walterlv.ForegroundWindowMonitor;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Timers;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using Timer = System.Timers.Timer;

namespace LanguageModeSwitcherWpf;

public class Monitor : IDisposable
{
    private Timer _refreshTimer;

    //应用程序名，是否为中文模式
    private Dictionary<string, bool> _dic = new Dictionary<string, bool>();

    private string? _timeTickerLastProgressName;
    private string? _processChangedLastProgressName;
    private GCHandle _callBackHandle;
    private HWINEVENTHOOK _hWINEVENTHOOK = new();

    public Monitor()
    {
        _refreshTimer = new Timer();
        _refreshTimer.Interval = Constant.RefreshDelay;
        _refreshTimer.Elapsed += RefreshTimerTick;
        _refreshTimer.Start();

        StartMonitForegroundWindowChange();
    }

    public void Dispose()
    {
        if (_callBackHandle.IsAllocated) _callBackHandle.Free();
        if (_hWINEVENTHOOK != 0) PInvoke.UnhookWinEvent(_hWINEVENTHOOK);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 定时获取输入法状态
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RefreshTimerTick(object? sender, ElapsedEventArgs e)
    {
        //跟上次获取到的程序名称对比，不一样什么都不做
        var progressName = GetForegroundWindowProgressName();
        if (_timeTickerLastProgressName != progressName)
        {
            _timeTickerLastProgressName = progressName;
            return;
        }

        var isChineseMode = CheckIsChineseMode(out _);
        if (_dic.TryGetValue(progressName, out bool storedIsChineseMode))
        {
            _dic[progressName] = isChineseMode;
        }
        else
        {
            _dic.Add(progressName, isChineseMode);
        }

        _timeTickerLastProgressName = progressName;
    }

    #region 监听应用程序切换
    // https://github.com/walterlv/Walterlv.ForegroundWindowMonitor

    /// <summary>
    /// 开始监听前景窗口切换
    /// </summary>
    private void StartMonitForegroundWindowChange()
    {
        try
        {
            // https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.gchandle?view=net-8.0
            var callBack = new WINEVENTPROC(WinEventCallBack);
            _callBackHandle = GCHandle.Alloc(callBack);
            // 监听系统的前台窗口变化。
            _hWINEVENTHOOK = PInvoke.SetWinEventHook(PInvoke.EVENT_SYSTEM_FOREGROUND,
                                                     PInvoke.EVENT_SYSTEM_FOREGROUND,
                                                     (HMODULE)IntPtr.Zero,
                                                     callBack,
                                                     0,
                                                     0,
                                                     PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS);

            // 开启消息循环，以便 WinEventProc 能够被调用。
            if (PInvoke.GetMessage(out var lpMsg, default, default, default))
            {
                PInvoke.TranslateMessage(in lpMsg);
                PInvoke.DispatchMessage(in lpMsg);
            }
        }
        catch (Exception)
        {
            if (_hWINEVENTHOOK != 0)
            {
                this.Dispose();
                Application.Current.Shutdown();
            }
        }
    }

    /// <summary>
    /// 当前台窗口变化时
    /// </summary>
    /// <param name="hWinEventHook"></param>
    /// <param name="event"></param>
    /// <param name="hwnd"></param>
    /// <param name="idObject"></param>
    /// <param name="idChild"></param>
    /// <param name="idEventThread"></param>
    /// <param name="dwmsEventTime"></param>
    private void WinEventCallBack(HWINEVENTHOOK hWinEventHook,
                                  uint @event,
                                  HWND hwnd,
                                  int idObject,
                                  int idChild,
                                  uint idEventThread,
                                  uint dwmsEventTime)
    {
        string? progressName1, progressName2;

        _refreshTimer.Stop();
        try
        {
            progressName1 = GetForegroundWindowProgressName();
            Thread.Sleep(200);
            progressName2 = GetForegroundWindowProgressName();
        }
        catch
        {
            return;
        }
        finally
        {
            _refreshTimer.Start();
        }

        if (progressName1 != progressName2 || progressName2 == _processChangedLastProgressName)
        {
            return;
        }

        var currentIsChinese = CheckIsChineseMode(out HWND imeHWND);

        if (_dic.TryGetValue(progressName2, out bool storedIsChinese))
        {
            if (storedIsChinese)
            {
                PInvoke.SendMessage(imeHWND, PInvoke.WM_IME_CONTROL, PInvoke.IMC_SETCONVERSIONMODE, IME_CMODE_CHINESE);
            }
            else
            {
                PInvoke.SendMessage(imeHWND, PInvoke.WM_IME_CONTROL, PInvoke.IMC_SETCONVERSIONMODE, IME_CMODE_ALPHANUMERIC);
            }
        }
        _processChangedLastProgressName = progressName2;

#if DEBUG
        Debug.WriteLine($@"{DateAndTime.Now:HH:mm:ss.fff}-{progressName2}-{currentIsChinese}=>{storedIsChinese}");
#endif
    }
    #endregion

    /// <summary>
    /// 获取前台窗口进程名
    /// </summary>
    /// <returns></returns>
    private string GetForegroundWindowProgressName()
    {
        var current = PInvoke.GetForegroundWindow();
        var w = new Win32Window(current);
        return w.ProcessName;
    }

    /// <summary>
    /// 当前输入是否为中文模式
    /// </summary>
    /// <param name="hWND"></param>
    /// <returns></returns>
    private bool CheckIsChineseMode(out HWND hWND)
    {
        return (GetImeConversionMode(out hWND) & IME_CMODE_NATIVE) != 0;
    }

    // https://github.com/ZGGSONG/LangIndicator

    /// <summary>
    /// 获取当前输入法的转换模式
    /// </summary>
    private int GetImeConversionMode(out HWND hWND)
    {
        hWND = HWND.Null;
        var foregroundWindow = PInvoke.GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero)
            return 0;

        hWND = PInvoke.ImmGetDefaultIMEWnd(foregroundWindow);
        if (hWND == IntPtr.Zero)
            return 0;

        var result = PInvoke.SendMessage(hWND, PInvoke.WM_IME_CONTROL, IMC_GETCONVERSIONMODE, IntPtr.Zero);
        return result.Value.ToInt32();
    }

    /// <summary>
    /// 获取大写锁定键状态
    /// </summary>
    /// <returns></returns>
    private int GetCapsLockState()
    {
        return PInvoke.GetKeyState(VK_CAPITAL) & 0x0001;
    }

    /// <summary>
    /// 大写锁定键
    /// </summary>
    private const int VK_CAPITAL = 0x14;

    /// <summary>
    /// 输入法管理器命令
    /// </summary>
    private const int IMC_GETCONVERSIONMODE = 0x001;

    #region IGP_CONVERSION
    // https://www.cnblogs.com/zyl910/archive/2006/06/04/2186644.html
    private const int IME_CMODE_ALPHANUMERIC = 0x0;     // 英文字母
    private const int IME_CMODE_CHINESE = 0x1;          // 中文输入
    private const int IME_CMODE_NATIVE = 0x1;           // 等同于 CHINESE
    private const int IME_CMODE_FULLSHAPE = 0x8;        // 全角
    private const int IME_CMODE_ROMAN = 0x10;           // 罗马字
    private const int IME_CMODE_CHARCODE = 0x20;        // 字符码
    private const int IME_CMODE_HANJACONVERT = 0x40;    // 汉字转换
    private const int IME_CMODE_SOFTKBD = 0x80;         // 软键盘
    private const int IME_CMODE_NOCONVERSION = 0x100;   // 无转换
    private const int IME_CMODE_EUDC = 0x200;           // 用户自定义字符
    private const int IME_CMODE_SYMBOL = 0x400;         // 符号转换
    private const int IME_CMODE_FIXED = 0x800;          // 固定转换
    #endregion
}
