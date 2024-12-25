using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Walterlv.ForegroundWindowMonitor;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Timers;
using System.Collections.Generic;
using System;
using Timer = System.Timers.Timer;
using System.Threading;

namespace LanguageModeSwitcherWpf;

public class Monitor
{
    private Timer _refreshTimer;

    //应用程序名，是否为中文模式
    private Dictionary<string, bool> _dic = new Dictionary<string, bool>();

    private string _timeTickerLastProgressName;
    private string _processChangedLastProgressName;

    public Monitor()
    {
        _refreshTimer = new Timer();
        _refreshTimer.Interval = Constant.RefreshDelay;
        _refreshTimer.Elapsed += RefreshTimerTick;
        _refreshTimer.Start();

        StartForegroundWindowMonitor();
    }

    #region 监听应用程序切换
    // https://github.com/walterlv/Walterlv.ForegroundWindowMonitor

    public void StartForegroundWindowMonitor()
    {
        // 监听系统的前台窗口变化。
        PInvoke.SetWinEventHook(PInvoke.EVENT_SYSTEM_FOREGROUND,
                                PInvoke.EVENT_SYSTEM_FOREGROUND,
                                HMODULE.Null,
                                WinEventProc,
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

    // 当前前台窗口变化时，输出新的前台窗口信息。
    internal void WinEventProc(HWINEVENTHOOK hWinEventHook,
                             uint @event,
                             HWND hwnd,
                             int idObject,
                             int idChild,
                             uint idEventThread,
                             uint dwmsEventTime)
    {
        //Alt+Tab会进入explorer
        _refreshTimer.Stop();
        var progressName1 = GetForegroundWindowProgressName();
        Thread.Sleep(200);
        var progressName2 = GetForegroundWindowProgressName();
        _refreshTimer.Start();
        if (progressName1 != progressName2)
        {
            return;
        }

        if (progressName2 == _processChangedLastProgressName)
        {
            //没有切换应用程序
            return;
        }

        //获取输入法状态
        var currentIsChinese = CheckIsChineseMode();

        if (_dic.TryGetValue(progressName2, out bool storedIsChinese))
        {
            var foregroundWindow = PInvoke.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return;

            var imeWnd = PInvoke.ImmGetDefaultIMEWnd(foregroundWindow);
            if (imeWnd == IntPtr.Zero)
                return;

            if (storedIsChinese)
            {
                PInvoke.SendMessage(imeWnd, PInvoke.WM_IME_CONTROL, PInvoke.IMC_SETCONVERSIONMODE, IME_CMODE_CHINESE);
            }
            else
            {
                PInvoke.SendMessage(imeWnd, PInvoke.WM_IME_CONTROL, PInvoke.IMC_SETCONVERSIONMODE, IME_CMODE_ALPHANUMERIC);
            }
        }

        _processChangedLastProgressName = progressName2;

#if DEBUG
        Debug.WriteLine($@"{DateAndTime.Now:HH: mm:ss.fff}-{progressName2}-{currentIsChinese}=>{storedIsChinese}");
#endif
    }
    #endregion

    public string GetForegroundWindowProgressName()
    {
        var current = PInvoke.GetForegroundWindow();
        var w = new Win32Window(current);
        return w.ProcessName;
    }

    public void RefreshTimerTick(object? sender, ElapsedEventArgs e)
    {
        //跟上次获取到的程序名称对比，不一样什么都不做
        var progressName = GetForegroundWindowProgressName();
        if (_timeTickerLastProgressName != progressName)
        {
            _timeTickerLastProgressName = progressName;
            return;
        }

        var isChineseMode = CheckIsChineseMode();
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

    private bool CheckIsChineseMode()
    {
        return (GetImeConversionMode() & IME_CMODE_NATIVE) != 0;
    }


    // https://github.com/ZGGSONG/LangIndicator

    /// <summary>
    /// 获取当前输入法的转换模式
    /// </summary>
    private int GetImeConversionMode()
    {
        var foregroundWindow = PInvoke.GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero)
            return 0;

        var imeWnd = PInvoke.ImmGetDefaultIMEWnd(foregroundWindow);
        if (imeWnd == IntPtr.Zero)
            return 0;

        var result = PInvoke.SendMessage(imeWnd, PInvoke.WM_IME_CONTROL, IMC_GETCONVERSIONMODE, IntPtr.Zero);
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

    private const int IME_CMODE_ALPHANUMERIC = 0x0;
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
