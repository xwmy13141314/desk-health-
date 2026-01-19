using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DeskHealth.Core.Interfaces;
using DeskHealth.App.Services;

namespace DeskHealth.App.Views;

/// <summary>
/// 主窗口（隐藏，仅作为应用生命周期载体）
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Visibility = Visibility.Hidden;
    }

    public void InitializeServices(IServiceProvider serviceProvider)
    {
        var timerService = serviceProvider.GetRequiredService<ITimerService>();
        var notifyService = serviceProvider.GetRequiredService<NotifyService>();
        var trayIconService = serviceProvider.GetRequiredService<TrayIconService>();

        // 初始化服务
        notifyService.Initialize();
        trayIconService.Initialize();

        // 订阅提醒事件
        timerService.ReminderTriggered += (s, e) =>
        {
            notifyService.ShowReminder(e.Reminder);
        };
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
    }
}
