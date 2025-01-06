using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using LanguageModeSwitcherWpf.Domain;
using LanguageModeSwitcherWpf.Extensions;
using LanguageModeSwitcherWpf.Services;
using LanguageModeSwitcherWpf.ViewMode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LanguageModeSwitcherWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskBarNotifyIcon _notifyIcon;
    private static Monitor _monitor;

    public static UnitWork<UserDataContext> UnitWork;
    public static Configs Configs;

    private readonly IHost host;

    public App()
    {
        //检查是否已经在运行
        if (IsRuned())
        {
            Environment.Exit(0);
        }

#if !DEBUG
        //绑定错误捕获
        DispatcherUnhandledException += App_DispatcherUnhandledException;
#endif

        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.RegisterAllServices();
        });

        UnitWork = new UnitWork<UserDataContext>(new UserDataContext());
        LoadConfigs();
    }

    #region 获取当前程序是否已运行
    /// <summary>
    /// 获取当前程序是否已运行
    /// </summary>
    private bool IsRuned()
    {
        string strProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        bool result = System.Diagnostics.Process.GetProcessesByName(strProcessName).Length > 1;
#if DEBUG
        result = false;
#endif
        return result;
    }
    #endregion

    #region 全局错误处理
    /// <summary>
    /// 全局错误处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        //todo 记录日志
        MessageBox.Show(e.Exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    #endregion

    /// <summary>
    /// 启动入口
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        DeleteUnlockrules();

        _notifyIcon = new TaskBarNotifyIcon();
        _monitor = new Monitor();
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
            }
        }

        App.Configs = new Configs();
    }

    public static void SaveConfigs()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(App.Configs);

        File.WriteAllTextAsync(Configs.ConfigFilePath, yaml);

        _monitor.UpdateInterval();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _monitor.Dispose();
        base.OnExit(e);
    }
}
