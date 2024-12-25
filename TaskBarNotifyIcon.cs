using System;
using System.Windows;
using System.Windows.Forms;

namespace LanguageModeSwitcherWpf;

public class TaskBarNotifyIcon
{
    //通知栏图标
    private NotifyIcon _notifyIcon;

    //设置窗口
    private Window? _mainWindow;

    public TaskBarNotifyIcon()
    {
        //设置托盘的各个属性
        _notifyIcon = new NotifyIcon
        {
            Text = Constant.Name,
            Icon = Resource.tq,
            Visible = true
        };

        //绑定鼠标点击事件
        _notifyIcon.MouseClick += new MouseEventHandler(notifyIcon_MouseClick);

        //退出菜单
        ContextMenuStrip contextMenu = new();
        contextMenu.Items.Add("退出", null, notifyIcon_exit);

        //托盘图标右键菜单
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void notifyIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        //显示设置窗口
        if (e.Button == MouseButtons.Left)
        {
            if (_mainWindow is null)
            {
                _mainWindow = new MainWindow();
                _mainWindow.Show();
            }
            else
            {
                _mainWindow.Close();
                _mainWindow = null;
                GC.Collect();
            }
        }
    }

    private void notifyIcon_exit(object? sender, EventArgs e)
    {
        _notifyIcon.Dispose();
        Environment.Exit(0);
    }

    public void ShowBalloonTip(string tip, ToolTipIcon tipIcon = ToolTipIcon.Info)
    {
        _notifyIcon.ShowBalloonTip(Constant.BalloonTipDelay, Constant.Name, tip, tipIcon);
    }
}
