using System;
using System.Windows;

namespace LanguageModeSwitcherWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskBarNotifyIcon? _notifyIcon;
    private Monitor? _switcher;

    public App()
    {
#if !DEBUG
        #region 检查是否已经运行
        string strProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        //检查进程是否已经启动，已经启动则显示报错信息退出程序。 
        if (System.Diagnostics.Process.GetProcessesByName(strProcessName).Length > 1)
        {
            MessageBox.Show("程序已经在运行！", "消息", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Environment.Exit(0);
            return;
        }

        //绑定错误捕获
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        #endregion
#endif
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        _notifyIcon = new TaskBarNotifyIcon();

        _switcher = new Monitor();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.ExceptionObject.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
