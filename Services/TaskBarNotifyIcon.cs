﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;

namespace LanguageModeSwitcherWpf.Services;

public class TaskBarNotifyIcon : IHostedService
{
    //通知栏图标
    private NotifyIcon _notifyIcon;

    //设置窗口
    private Window _mainWindow;

    public TaskBarNotifyIcon()
    {
        _mainWindow = new MainWindow();

        //设置托盘的各个属性
        _notifyIcon = new NotifyIcon
        {
            Text = Configs.Name,
            Icon = Resource.tq,
            Visible = true
        };

        //绑定鼠标点击事件
        _notifyIcon.MouseClick += new MouseEventHandler(_notifyIcon_MouseClick);

        var menuContext = new ContextMenuStrip();
        menuContext.Items.Add("设置", null, (sender, e) =>
        {
            _mainWindow.Show();
            _mainWindow.Activate();
        });
        menuContext.Items.Add("退出", null, notifyIcon_exit);

        _notifyIcon.ContextMenuStrip = menuContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private void _notifyIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        //显示设置窗口
        if (e.Button == MouseButtons.Left)
        {
            if (_mainWindow.Visibility != Visibility.Visible)
            {
                _mainWindow.Show();
                _mainWindow.Activate();
            }
            else
            {
                _mainWindow.Hide();
            }
        }
    }

    private void notifyIcon_exit(object? sender, EventArgs e)
    {
        _notifyIcon.Dispose();
        System.Windows.Application.Current.Shutdown();
    }
}
