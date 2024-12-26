using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Timers;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using Timer = System.Timers.Timer;
using Walterlv.ForegroundWindowMonitor;

namespace LanguageModeSwitcherWpf;

/// <summary>
/// 监控器
/// </summary>
public class Monitor : IDisposable
{
    private Timer _refreshTimer;

    //应用程序名，是否为中文模式
    private Dictionary<string, bool> _dic = new Dictionary<string, bool>();


    private GCHandle _callBackHandle;
    private HWINEVENTHOOK _hWINEVENTHOOK = new();

    public Monitor()
    {
        _refreshTimer = new Timer
        {
            Interval = Constant.RefreshDelay
        };
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

    private string? _lastProgressName_TimeTicker;
    private bool? _lastIsChineseMode_TimeTicker;
    /// <summary>
    /// 定时获取输入法状态
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RefreshTimerTick(object? sender, ElapsedEventArgs e)
    {
        //跟上次获取到的程序名称对比，不一样什么都不做
        Win32Window window = Win32Helper.GetForegroundWindowInfo();
        if (_lastProgressName_TimeTicker != window.ProcessName)
        {
            _lastProgressName_TimeTicker = window.ProcessName;
            return;
        }

        var isChineseMode = Win32Helper.GetIsChineseInputMode(window.IMEHandle);

        //输入模式没变，直接返回
        if (_lastIsChineseMode_TimeTicker == isChineseMode)
        {
            return;
        }

        //输入模式变了，存起来
        if (_dic.TryGetValue(window.ProcessName, out bool storedIsChineseMode))
        {
            _dic[window.ProcessName] = isChineseMode;
        }
        else
        {
            _dic.Add(window.ProcessName, isChineseMode);
        }

        _lastProgressName_TimeTicker = window.ProcessName;
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

    private string? _lastProgressName_ProcessChanged;
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
        Win32Window? window1, window2;

        _refreshTimer.Stop();
        try
        {
            window1 = Win32Helper.GetForegroundWindowInfo();
            Thread.Sleep(200);
            window2 = Win32Helper.GetForegroundWindowInfo();
        }
        catch
        {
            return;
        }
        finally
        {
            _refreshTimer.Start();
        }

        if (window1.ProcessName != window2.ProcessName || window2.ProcessName == _lastProgressName_ProcessChanged)
        {
            return;
        }
        bool currentIsChinese = Win32Helper.GetIsChineseInputMode(window2.IMEHandle);

        if (_dic.TryGetValue(window2.ProcessName, out bool storedIsChinese))
        {
            if (storedIsChinese)
            {
                Win32Helper.SwitchToChineseMode(window2.IMEHandle);
            }
            else
            {
                Win32Helper.SwitchToAlphaNumericMode(window2.IMEHandle);
            }
        }
        _lastProgressName_ProcessChanged = window2.ProcessName;

#if DEBUG
        Debug.WriteLine($@"{DateAndTime.Now:HH:mm:ss.fff}-{window2.ProcessName}-{currentIsChinese}=>{storedIsChinese}");
#endif
    }
    #endregion
}
