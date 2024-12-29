using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageModeSwitcherWpf.Helper;
using LanguageModeSwitcherWpf.Models.Domain;

namespace LanguageModeSwitcherWpf;

partial class MainViewMode : ObservableObject
{
    /// <summary>
    /// 是否开机自启
    /// </summary>
    [ObservableProperty]
    private bool _isStartUp;

    [ObservableProperty]
    private List<Rules> _rules;

    [ObservableProperty]
    private Configs _configs;

    [RelayCommand]
    private void VisiblityChanged(Visibility visibility)
    {
        //刷新显示
        if (visibility == Visibility.Visible)
        {
            IsStartUp = ShortcutUtilities.IsStartup();

            Rules = App.UnitWork.Find<Rules>().OrderBy(p => p.Id).ThenBy(p => p.Lock).ToList();

            App.LoadConfigs();
            Configs = App.Configs;
        }
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    [RelayCommand]
    private void Save(Window window)
    {
        App.Configs = Configs;
        App.UnitWork.BulkUpdate(Rules);
        App.SaveConfigs();
        window.Close();
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        window.Close();
    }
}
