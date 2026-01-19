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

    public NotifyService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize()
    {
        // 预加载窗口
        _window = _serviceProvider.GetRequiredService<NotifyWindow>();
    }

    public void ShowReminder(Reminder reminder)
    {
        if (_window == null)
        {
            _window = _serviceProvider.GetRequiredService<NotifyWindow>();
        }

        _window.ShowReminder(reminder);
    }

    public void HideReminder()
    {
        _window?.HideReminder();
    }
}
