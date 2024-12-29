using System;
using System.IO;
using System.Linq;
using System.Windows;
using LanguageModeSwitcherWpf.Models;
using LanguageModeSwitcherWpf.Models.Domain;
using LanguageModeSwitcherWpf.View;
using LanguageModeSwitcherWpf.ViewMode;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LanguageModeSwitcherWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskBarNotifyIcon _notifyIcon;
    private Monitor _monitor;

    public static UnitWork<UserDataContext> UnitWork;
    public static Configs Configs;

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

        UnitWork = new UnitWork<UserDataContext>(new UserDataContext());

        LoadConfigs();

        DeleteUnlockrules();
    }

    private void DeleteUnlockrules()
    {
        if (Configs.DeleteUnlockRules)
        {
            var deleteEntyties = UnitWork.Finds<Rules>(p => p.Lock == false && p.Id != 1).ToList();
            if (deleteEntyties.Count() > 0)
            {
                UnitWork.BulkDelete(deleteEntyties);
                UnitWork.Save();
            }
        }
    }

    public static void LoadConfigs()
    {
        if (File.Exists(Configs.ConfigFilePath))
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
            try
            {
                App.Configs = deserializer.Deserialize<Configs>(File.ReadAllText(Configs.ConfigFilePath));
                return;
            }
            catch (Exception)
            {
                File.Delete(Configs.ConfigFilePath);
                App.Configs = new Configs();
            }
        }
    }

    public static void SaveConfigs()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(App.Configs);

        File.WriteAllTextAsync(Configs.ConfigFilePath, yaml);
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        _notifyIcon = new TaskBarNotifyIcon();
        _monitor = new Monitor();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _monitor.Dispose();
        base.OnExit(e);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.ExceptionObject.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
