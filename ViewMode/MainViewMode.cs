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

    [RelayCommand]
    private void VisiblityChanged(Visibility visibility)
    {
        //刷新显示
        if (visibility == Visibility.Visible)
        {
            IsStartUp = ShortcutUtilities.IsStartup();
            Rules = App.UnitWork.Find<Rules>().ToList();
        }
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    [RelayCommand]
    private void Save()
    {

    }

    [RelayCommand]
    private void Cancel()
    {

    }
}
