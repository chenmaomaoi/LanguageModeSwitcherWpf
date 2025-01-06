using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using LanguageModeSwitcherWpf.Helper;
using LanguageModeSwitcherWpf.Common;
using LanguageModeSwitcherWpf.Domain;

namespace LanguageModeSwitcherWpf.ViewMode;

/// <summary>
/// 监控器
/// </summary>
public class Monitor : IDisposable
{
    private DispatcherTimer _refreshTimer;

    private GCHandle _callBackHandle;
    private HWINEVENTHOOK _hWINEVENTHOOK = new();

    public Monitor()
    {
        _refreshTimer = new();
        UpdateInterval();
        _refreshTimer.Tick += RefreshTimerTick;
        StartMonitForegroundWindowChange();
        _refreshTimer.Start();
    }

    public void UpdateInterval()
    {
        _refreshTimer.Interval = TimeSpan.FromMilliseconds(App.Configs.RefreshDelay * 100);
    }

    public void Dispose()
    {
        if (_callBackHandle.IsAllocated) _callBackHandle.Free();
        if (_hWINEVENTHOOK != 0) PInvoke.UnhookWinEvent(_hWINEVENTHOOK);

        GC.SuppressFinalize(this);
    }

    private string? _lastProgressName_TimeTicker;
    private IMECode _lastIMECode_TimeTicker;
    /// <summary>
    /// 定时获取输入法状态
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RefreshTimerTick(object? sender, EventArgs e)
    {
        //跟上次获取到的程序名称对比，不一样什么都不做
        Win32Window window = Win32Helper.GetForegroundWindowInfo();
        if (_lastProgressName_TimeTicker != window.ProcessName)
        {
            _lastProgressName_TimeTicker = window.ProcessName;
            return;
        }

        var currentIMECode = Win32Helper.GetIMECode(window.IMEHandle);

        //IMECode相比上次没变，直接返回
        if (_lastIMECode_TimeTicker == currentIMECode)
        {
            return;
        }

        //变了，查询数据库
        var record = App.UnitWork.FirstOrDefault<Rules>(p => p.ProgressName == window.ProcessName);

        if (record == default)
        {
            //库里没有
            //看默认设置
            var defaultRule = App.UnitWork.FirstOrDefault<Rules>(p => p.Id == 1);
            if (defaultRule.MonitIMECodeChanges)
            {
                //按默认设置新增
                record = new()
                {
                    ProgressName = window.ProcessName,
                    MonitIMECodeChanges = defaultRule.MonitIMECodeChanges,
                    IMECode = currentIMECode,
                    Lock = defaultRule.Lock
                };

                App.UnitWork.Add(record);
                App.UnitWork.Save();
            }
        }
        else
        {
            //库里有
            if (record.MonitIMECodeChanges && record.IMECode != currentIMECode)
            {
                //与存的不符，改库
                record.IMECode = currentIMECode;
                App.UnitWork.Update(record);
                App.UnitWork.Save();
            }
        }

        _lastProgressName_TimeTicker = window.ProcessName;
        _lastIMECode_TimeTicker = currentIMECode;
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
                                                     (HMODULE)nint.Zero,
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
                Dispose();
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
        IMECode currentIMECode = Win32Helper.GetIMECode(window2.IMEHandle);

        var record = App.UnitWork.FirstOrDefault<Rules>(p => p.ProgressName == window2.ProcessName);

        if (record != default && record.IMECode != IMECode.Ignore)
        {
            Win32Helper.SetIMECode(window2.IMEHandle, record.IMECode);
        }
        _lastProgressName_ProcessChanged = window2.ProcessName;

#if DEBUG
        Debug.WriteLine($@"{DateAndTime.Now:HH:mm:ss.fff}-{window2.ProcessName}-{currentIMECode}=>{record?.IMECode.ToString()}");
#endif
    }
    #endregion
}
