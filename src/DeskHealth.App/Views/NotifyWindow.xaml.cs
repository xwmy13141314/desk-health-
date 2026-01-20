using System.Diagnostics;
using System.IO;
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
    private readonly string _logFile;

    public NotifyWindow()
    {
        InitializeComponent();

        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeskHealth");
        Directory.CreateDirectory(logDir);
        _logFile = Path.Combine(logDir, "debug.log");

        _autoCloseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10) // 默认10秒
        };
        _autoCloseTimer.Tick += OnAutoCloseTimerTick;

        Loaded += (s, e) => Log("[NotifyWindow] Window loaded");
    }

    private void Log(string message)
    {
        var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        Debug.WriteLine(logMessage);
        try
        {
            File.AppendAllText(_logFile, logMessage + "\n");
        }
        catch { }
    }

    /// <summary>
    /// 设置提醒内容并显示窗口
    /// </summary>
    public void ShowReminder(Reminder reminder)
    {
        Log($"[NotifyWindow] ShowReminder called: {reminder.Title}");

        DataContext = reminder;

        // 根据提醒类型调整UI
        if (reminder is CombinedReminder combinedReminder)
        {
            ShowCombinedReminder(combinedReminder);
        }
        else
        {
            ShowSingleReminder(reminder);
        }

        // 设置自动关闭时间
        _autoCloseTimer.Interval = TimeSpan.FromSeconds(reminder.AutoCloseSeconds);

        // 确保窗口可见
        Visibility = Visibility.Visible;

        // 确保窗口在最顶层并激活以吸引注意
        Topmost = true;

        // 显示窗口（XAML 中已设置 WindowStartupLocation="CenterScreen"）
        Log("[NotifyWindow] Calling Show()...");
        Show();

        Log($"[NotifyWindow] Window position: Left={Left}, Top={Top}, Width={Width}, Height={Height}");
        Log($"[NotifyWindow] Window visibility: {Visibility}, IsVisible={IsVisible}, WindowState={WindowState}");

        // 强制激活窗口并设置焦点
        Log("[NotifyWindow] Calling Activate()...");
        var activateResult = Activate();
        Log($"[NotifyWindow] Activate() returned: {activateResult}");

        Focus();
        Log("[NotifyWindow] Focus() called");

        // 启动自动关闭计时器
        _autoCloseTimer.Start();

        Log($"[NotifyWindow] ShowReminder completed, IsVisible={IsVisible}");
    }

    private void ShowSingleReminder(Reminder reminder)
    {
        Log("[NotifyWindow] Showing single reminder");
        SingleReminderPanel.Visibility = Visibility.Visible;
        CombinedReminderPanel.Visibility = Visibility.Collapsed;
        Height = 180; // 单个提醒窗口高度
    }

    private void ShowCombinedReminder(CombinedReminder combinedReminder)
    {
        Log($"[NotifyWindow] Showing combined reminder with {combinedReminder.SubReminders.Count} sub-reminders");

        SingleReminderPanel.Visibility = Visibility.Collapsed;
        CombinedReminderPanel.Visibility = Visibility.Visible;

        // 设置子提醒列表数据源
        SubRemindersItemsControl.ItemsSource = combinedReminder.SubReminders;

        // 根据子提醒数量调整窗口高度
        // 基础高度 200 + 每个子提醒 60
        var newHeight = 200 + (combinedReminder.SubReminders.Count * 60);
        Height = Math.Min(newHeight, 500); // 最大高度限制为 500

        Log($"[NotifyWindow] Combined reminder window height: {Height}");
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
    {
        Log("[NotifyWindow] Close button clicked");
        HideReminder();
    }

    private void OnAutoCloseTimerTick(object? sender, EventArgs e)
    {
        Log("[NotifyWindow] Auto close timer ticked");
        _autoCloseTimer.Stop();
        HideReminder();
    }

    public void HideReminder()
    {
        Log("[NotifyWindow] HideReminder called");
        _autoCloseTimer.Stop();
        Hide();
    }

    protected override void OnClosed(EventArgs e)
    {
        Log("[NotifyWindow] Window closed");
        _autoCloseTimer.Stop();
        base.OnClosed(e);
    }
}
