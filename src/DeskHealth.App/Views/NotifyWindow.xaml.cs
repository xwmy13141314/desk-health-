using System.Windows;
using System.Windows.Threading;
using DeskHealth.Core.Entities;

namespace DeskHealth.App.Views;

/// <summary>
/// 半透明提醒窗口
/// </summary>
public partial class NotifyWindow : Window
{
    private readonly DispatcherTimer _autoCloseTimer;

    public NotifyWindow()
    {
        InitializeComponent();

        _autoCloseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10) // 默认10秒
        };
        _autoCloseTimer.Tick += OnAutoCloseTimerTick;
    }

    /// <summary>
    /// 设置提醒内容并显示窗口
    /// </summary>
    public void ShowReminder(Reminder reminder)
    {
        DataContext = reminder;

        // 设置自动关闭时间
        _autoCloseTimer.Interval = TimeSpan.FromSeconds(reminder.AutoCloseSeconds);

        // 计算屏幕右下角位置
        PositionWindowAtBottomRight();

        // 关键：不激活窗口，不抢焦点
        ShowActivated = false;
        Show();

        // 启动自动关闭计时器
        _autoCloseTimer.Start();
    }

    /// <summary>
    /// 将窗口定位到屏幕右下角
    /// </summary>
    private void PositionWindowAtBottomRight()
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var taskbarHeight = GetTaskbarHeight();

        Left = screenWidth - Width - 10;
        Top = screenHeight - taskbarHeight - Height - 10;
    }

    /// <summary>
    /// 获取任务栏高度
    /// </summary>
    private double GetTaskbarHeight()
    {
        var workingArea = SystemParameters.WorkArea;
        if (workingArea.Bottom < SystemParameters.PrimaryScreenHeight)
        {
            return SystemParameters.PrimaryScreenHeight - workingArea.Bottom;
        }
        return 40; // 默认值
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
    {
        HideReminder();
    }

    private void OnAutoCloseTimerTick(object? sender, EventArgs e)
    {
        _autoCloseTimer.Stop();
        HideReminder();
    }

    public void HideReminder()
    {
        _autoCloseTimer.Stop();
        Hide();
    }

    protected override void OnClosed(EventArgs e)
    {
        _autoCloseTimer.Stop();
        base.OnClosed(e);
    }
}
