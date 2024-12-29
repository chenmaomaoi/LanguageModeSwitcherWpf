using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AutoMapper;
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

    private IMapper _mapper;

    public MainViewMode()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Rules, Rules>());
        _mapper = config.CreateMapper();
    }

    [RelayCommand]
    private void VisiblityChanged(Visibility visibility)
    {
        if (visibility == Visibility.Visible)
        {
            IsStartUp = ShortcutUtilities.IsStartup();

            var rules = App.UnitWork.Find<Rules>().OrderBy(p => p.Id).ThenBy(p => p.Lock).ToList();
            Rules = _mapper.Map<List<Rules>>(rules);

            App.LoadConfigs();
            Configs = App.Configs;
        }
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    [RelayCommand]
    private void Save(DataGrid dataGrid)
    {
        App.Configs = Configs;

        var db = App.UnitWork.Finds<Rules>();

        foreach (Rules item in db)
        {
            if (!Rules.Any(p => p.Id == item.Id))
            {
                App.UnitWork.Delete(item);
            }
        }

        foreach (Rules item in Rules)
        {
            if (item.Id == 0 && !string.IsNullOrEmpty(item.ProgressName))
            {
                App.UnitWork.Add(item);
            }
        }

        App.UnitWork.BulkUpdate(Rules);
        App.UnitWork.Save();

        App.SaveConfigs();

        if (IsStartUp)
        {
            ShortcutUtilities.SetStartup();
        }
        else
        {
            ShortcutUtilities.UnSetStartup();
        }

        MessageBox.Show("OK");
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        window.Close();
    }

    [RelayCommand]
    private void Remove(DataGrid dataGrid)
    {
        if (((Rules)dataGrid.SelectedItem).Id == 1)
        {
            MessageBox.Show("默认规则不允许删除", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Rules.Remove((Rules)dataGrid.SelectedItem);
        Application.Current.Dispatcher.Invoke(dataGrid.Items.Refresh);
    }

    [RelayCommand]
    private void Add(DataGrid dataGrid)
    {
        Rules.Add(new());
        Application.Current.Dispatcher.Invoke(dataGrid.Items.Refresh);
    }

    [RelayCommand]
    private void Refresh(DataGrid dataGrid)
    {
        var rules = App.UnitWork.Find<Rules>().OrderBy(p => p.Id).ThenBy(p => p.Lock).ToList();
        Rules = _mapper.Map<List<Rules>>(rules);
        Application.Current.Dispatcher.Invoke(dataGrid.Items.Refresh);
    }
}
