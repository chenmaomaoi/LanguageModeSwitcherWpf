using System;
using Walterlv.ForegroundWindowMonitor;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace LanguageModeSwitcherWpf;

public static class Win32Helper
{
    #region 窗口
    /// <summary>
    /// 获取前台窗口信息
    /// </summary>
    /// <returns></returns>
    public static Win32Window GetForegroundWindowInfo()
    {
        var current = PInvoke.GetForegroundWindow();
        var w = new Win32Window(current);
        return w;
    }
    #endregion

    #region 输入法

    #region 大小写
    /// <summary>
    /// 获取大小写锁定状态
    /// </summary>
    /// <param name="hWND"></param>
    /// <returns></returns>
    internal static bool GetCapsLockState()
    {
        return (PInvoke.GetKeyState(VK_CAPITAL) & 0x0001) == 1;
    }
    #endregion

    #region 英文
    /// <summary>
    /// 
    /// </summary>
    /// <param name="imeHWND"></param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void SwitchToAlphaNumericMode(HWND imeHWND)
    {
        if (imeHWND.IsNull || imeHWND.Value == 0) return;
        PInvoke.SendMessage(imeHWND, PInvoke.WM_IME_CONTROL, PInvoke.IMC_SETCONVERSIONMODE, IME_CMODE_ALPHANUMERIC);
    }
    #endregion

    #region 中文
    /// <summary>
    /// 切换到中文输入
    /// </summary>
    /// <param name="imeHWND"></param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void SwitchToChineseMode(HWND imeHWND)
    {
        if (imeHWND.IsNull || imeHWND.Value == 0) return;
        PInvoke.SendMessage(imeHWND, PInvoke.WM_IME_CONTROL, PInvoke.IMC_SETCONVERSIONMODE, IME_CMODE_CHINESE);
    }

    /// <summary>
    /// 前台输入法是否为中文输入模式
    /// </summary>
    /// <returns></returns>
    internal static bool GetIsChineseInputMode_ForegroundWindow()
    {
        var hWID = GetForegroundIMEHWND();
        return GetIsChineseInputMode(hWID);
    }

    /// <summary>
    /// 输入法是否为中文模式
    /// </summary>
    /// <param name="imeHWND"></param>
    /// <returns></returns>
    internal static bool GetIsChineseInputMode(HWND imeHWND)
    {
        var mode = GetIMEConversionMode(imeHWND);
        return (mode & IME_CMODE_NATIVE) != 0;
    }
    #endregion

    /// <summary>
    /// 获取输入法转换模式
    /// </summary>
    /// <param name="imeHWND"></param>
    /// <returns></returns>
    private static LRESULT GetIMEConversionMode(HWND imeHWND)
    {
        if (imeHWND.IsNull || imeHWND.Value == 0) return (LRESULT)0;
        return PInvoke.SendMessage(imeHWND, PInvoke.WM_IME_CONTROL, IMC_GETCONVERSIONMODE, IntPtr.Zero);
    }

    /// <summary>
    /// 获取前台 窗口句柄
    /// </summary>
    /// <returns></returns>
    internal static HWND GetForegroundIMEHWND()
    {
        var foregroundWindow = PInvoke.GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero) return HWND.Null;

        return GetIMEHWND(foregroundWindow);
    }

    /// <summary>
    /// 获取窗口 IME句柄
    /// </summary>
    /// <param name="windowHWND"></param>
    /// <returns></returns>
    internal static HWND GetIMEHWND(HWND windowHWND)
    {
        var hWND = PInvoke.ImmGetDefaultIMEWnd(windowHWND);
        if (hWND == IntPtr.Zero) return HWND.Null;
        return hWND;
    }

    /// <summary>
    /// 大写锁定键
    /// </summary>
    internal const int VK_CAPITAL = 0x14;

    /// <summary>
    /// 输入法管理器命令
    /// </summary>
    internal const int IMC_GETCONVERSIONMODE = 0x001;

    #region IGP_CONVERSION
    // https://www.cnblogs.com/zyl910/archive/2006/06/04/2186644.html
    internal const int IME_CMODE_ALPHANUMERIC = 0x0;     // 英文字母
    internal const int IME_CMODE_CHINESE = 0x1;          // 中文输入
    internal const int IME_CMODE_NATIVE = 0x1;           // 等同于 CHINESE
    internal const int IME_CMODE_FULLSHAPE = 0x8;        // 全角
    internal const int IME_CMODE_ROMAN = 0x10;           // 罗马字
    internal const int IME_CMODE_CHARCODE = 0x20;        // 字符码
    internal const int IME_CMODE_HANJACONVERT = 0x40;    // 汉字转换
    internal const int IME_CMODE_SOFTKBD = 0x80;         // 软键盘
    internal const int IME_CMODE_NOCONVERSION = 0x100;   // 无转换
    internal const int IME_CMODE_EUDC = 0x200;           // 用户自定义字符
    internal const int IME_CMODE_SYMBOL = 0x400;         // 符号转换
    internal const int IME_CMODE_FIXED = 0x800;          // 固定转换
    #endregion
    #endregion
}
