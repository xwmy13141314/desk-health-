using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DeskHealth.Core.Entities;
using DeskHealth.Core.Interfaces;
using DeskHealth.App.Views;

namespace DeskHealth.App.Services;

/// <summary>
/// 提醒显示服务
/// </summary>
public class NotifyService : INotifyService
{
    private readonly IServiceProvider _serviceProvider;
    private NotifyWindow? _window;
    private readonly string _logFile;

    public NotifyService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeskHealth");
        Directory.CreateDirectory(logDir);
        _logFile = Path.Combine(logDir, "debug.log");
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

    public void Initialize()
    {
        Log("[NotifyService] Initialize called");
        // 在 UI 线程上预加载窗口
        if (Application.Current.Dispatcher.CheckAccess())
        {
            _window = _serviceProvider.GetRequiredService<NotifyWindow>();
            Log("[NotifyService] Window created (on UI thread)");
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _window = _serviceProvider.GetRequiredService<NotifyWindow>();
                Log("[NotifyService] Window created (dispatched to UI thread)");
            });
        }
    }

    public void ShowReminder(Reminder reminder)
    {
        Log($"[NotifyService] ShowReminder called: {reminder.Title}, thread={Thread.CurrentThread.ManagedThreadId}");
        Log($"[NotifyService] Dispatcher.CheckAccess={Application.Current.Dispatcher.CheckAccess()}");

        // 使用 Application.Current.Dispatcher 确保在 UI 线程上执行
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Log("[NotifyService] Dispatching to UI thread...");
            Application.Current.Dispatcher.Invoke(() => ShowReminderOnUIThread(reminder));
        }
        else
        {
            Log("[NotifyService] Already on UI thread, calling directly...");
            ShowReminderOnUIThread(reminder);
        }
    }

    private void ShowReminderOnUIThread(Reminder reminder)
    {
        Log($"[NotifyService] ShowReminderOnUIThread: creating NEW window instance");

        // 每次都创建新窗口，避免窗口状态问题
        var window = _serviceProvider.GetRequiredService<NotifyWindow>();

        Log($"[NotifyService] Calling ShowReminder on new window");
        window.ShowReminder(reminder);
        Log("[NotifyService] ShowReminder returned");
    }

    public void HideReminder()
    {
        Log("[NotifyService] HideReminder called");

        // 使用 Application.Current.Dispatcher 确保在 UI 线程上执行
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(HideReminderOnUIThread);
        }
        else
        {
            HideReminderOnUIThread();
        }
    }

    private void HideReminderOnUIThread()
    {
        _window?.HideReminder();
    }
}
