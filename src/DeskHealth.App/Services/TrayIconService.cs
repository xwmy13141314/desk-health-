using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DeskHealth.Core.Entities;
using DeskHealth.Core.Interfaces;
using TaskbarIcon = Hardcodet.Wpf.TaskbarNotification.TaskbarIcon;
using DeskHealth.App.Views;

namespace DeskHealth.App.Services;

/// <summary>
/// 系统托盘服务
/// </summary>
public class TrayIconService : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITimerService _timerService;
    private TaskbarIcon? _taskbarIcon;

    public TrayIconService(IServiceProvider serviceProvider, ITimerService timerService)
    {
        _serviceProvider = serviceProvider;
        _timerService = timerService;
    }

    public void Initialize()
    {
        // 创建托盘图标
        _taskbarIcon = new TaskbarIcon
        {
            Icon = LoadCustomIcon(),
            ToolTipText = "DeskHealth - 桌面健康提醒"
        };

        // 创建上下文菜单
        CreateContextMenu();

        // 订阅状态变化事件
        _timerService.StateChanged += OnTimerStateChanged;
    }

    /// <summary>
    /// 从嵌入资源加载自定义图标
    /// </summary>
    private Icon LoadCustomIcon()
    {
        try
        {
            // 尝试从嵌入资源加载
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "DeskHealth.App.tubiao.22222.png";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var bitmap = new Bitmap(stream))
                    {
                        // 调整大小为标准图标尺寸
                        var sizedBitmap = new Bitmap(bitmap, new Size(16, 16));
                        return Icon.FromHandle(sizedBitmap.GetHicon());
                    }
                }
            }
        }
        catch
        {
            // 如果从资源加载失败，尝试从文件加载
        }

        try
        {
            // 备用方案：从 tubiao 文件夹加载
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var iconPath = Path.Combine(baseDir, "tubiao", "22222.png");

            if (File.Exists(iconPath))
            {
                using (var bitmap = new Bitmap(iconPath))
                {
                    var sizedBitmap = new Bitmap(bitmap, new Size(16, 16));
                    return Icon.FromHandle(sizedBitmap.GetHicon());
                }
            }
        }
        catch
        {
            // 如果加载失败，使用默认图标
        }

        // 最终备用方案：使用系统默认图标
        return System.Drawing.SystemIcons.Application;
    }

    private void CreateContextMenu()
    {
        if (_taskbarIcon == null) return;

        var contextMenu = new System.Windows.Controls.ContextMenu();

        // 标题项（不可点击）
        var titleItem = new System.Windows.Controls.MenuItem
        {
            Header = $"DeskHealth v{GetVersion()}",
            FontWeight = System.Windows.FontWeights.Bold,
            IsEnabled = false
        };
        contextMenu.Items.Add(titleItem);
        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        // 测试提醒
        var testHydrationItem = new System.Windows.Controls.MenuItem
        {
            Header = "测试喝水提醒"
        };
        testHydrationItem.Click += (s, e) => TestReminder(ReminderType.Hydration);
        contextMenu.Items.Add(testHydrationItem);

        var testBreakItem = new System.Windows.Controls.MenuItem
        {
            Header = "测试休息提醒"
        };
        testBreakItem.Click += (s, e) => TestReminder(ReminderType.Break);
        contextMenu.Items.Add(testBreakItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        // 关于
        var aboutItem = new System.Windows.Controls.MenuItem
        {
            Header = "关于 DeskHealth..."
        };
        aboutItem.Click += (s, e) => ShowAboutWindow();
        contextMenu.Items.Add(aboutItem);

        // 暂停/恢复
        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var pause1hItem = new System.Windows.Controls.MenuItem
        {
            Header = "暂停提醒 1 小时"
        };
        pause1hItem.Click += (s, e) => PauseReminder(1);
        contextMenu.Items.Add(pause1hItem);

        var pause2hItem = new System.Windows.Controls.MenuItem
        {
            Header = "暂停提醒 2 小时"
        };
        pause2hItem.Click += (s, e) => PauseReminder(2);
        contextMenu.Items.Add(pause2hItem);

        var resumeItem = new System.Windows.Controls.MenuItem
        {
            Header = "恢复提醒",
            Name = "ResumeItem"
        };
        resumeItem.Click += (s, e) => ResumeReminder();
        contextMenu.Items.Add(resumeItem);

        // 退出
        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem
        {
            Header = "退出"
        };
        exitItem.Click += (s, e) => ExitApplication();
        contextMenu.Items.Add(exitItem);

        _taskbarIcon.ContextMenu = contextMenu;

        // 更新菜单状态
        UpdateMenuState();
    }

    private string GetVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString() ?? "1.0.0";
    }

    private void PauseReminder(int hours)
    {
        _timerService.Pause(TimeSpan.FromHours(hours));
        UpdateMenuState();
    }

    private void ResumeReminder()
    {
        _timerService.Resume();
        UpdateMenuState();
    }

    private void UpdateMenuState()
    {
        if (_taskbarIcon?.ContextMenu == null) return;

        var resumeItem = FindMenuItem(_taskbarIcon.ContextMenu, "ResumeItem");
        if (resumeItem != null)
        {
            resumeItem.Visibility = _timerService.State == TimerState.Running
                ? System.Windows.Visibility.Collapsed
                : System.Windows.Visibility.Visible;

            if (_timerService.State != TimerState.Running)
            {
                resumeItem.Header = "恢复提醒";
            }
        }
    }

    private System.Windows.Controls.MenuItem? FindMenuItem(System.Windows.Controls.ContextMenu menu, string name)
    {
        foreach (var item in menu.Items.OfType<System.Windows.Controls.MenuItem>())
        {
            if (item.Name == name)
                return item;
        }
        return null;
    }

    private void ShowAboutWindow()
    {
        var window = _serviceProvider.GetRequiredService<AboutWindow>();
        window.ShowDialog();
    }

    private void TestReminder(ReminderType type)
    {
        Debug.WriteLine($"[TrayIconService] TestReminder called with type={type}");

        // 获取配置服务并触发测试提醒
        var configService = _serviceProvider.GetRequiredService<IConfigService>();
        var reminder = configService.GetReminder(type);

        Debug.WriteLine($"[TrayIconService] Got reminder: {reminder.Title}");

        var notifyService = _serviceProvider.GetRequiredService<INotifyService>();
        Debug.WriteLine($"[TrayIconService] Calling notifyService.ShowReminder...");

        notifyService.ShowReminder(reminder);

        Debug.WriteLine($"[TrayIconService] TestReminder completed");
    }

    private void ExitApplication()
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void OnTimerStateChanged(object? sender, TimerState newState)
    {
        // 在 UI 线程上更新菜单
        if (_taskbarIcon?.Dispatcher != null)
        {
            _taskbarIcon.Dispatcher.Invoke(UpdateMenuState);
        }
    }

    public void Dispose()
    {
        _timerService.StateChanged -= OnTimerStateChanged;
        _taskbarIcon?.Dispose();
    }
}
