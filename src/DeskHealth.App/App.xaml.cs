using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DeskHealth.Core.Interfaces;
using DeskHealth.Services;
using DeskHealth.App.Services;
using DeskHealth.App.Views;

namespace DeskHealth.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string MutexName = "Global\\DeskHealth_SingleInstanceMutex";
    private Mutex? _mutex;
    public IServiceProvider? Services { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 单例检查
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "DeskHealth 已经在运行中。",
                "提示",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // 配置依赖注入
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // 初始化并启动主窗口（隐藏）
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.InitializeServices(Services);
        mainWindow.Visibility = Visibility.Hidden;
        mainWindow.Show();

        // 启动计时器服务
        var timerService = Services.GetRequiredService<ITimerService>();
        timerService.Start();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 核心服务
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<ITimerService, TimerService>();

        // 应用服务
        services.AddSingleton<INotifyService, NotifyService>();
        services.AddSingleton<NotifyService>();
        services.AddSingleton<TrayIconService>();

        // 窗口
        services.AddSingleton<MainWindow>();
        services.AddTransient<NotifyWindow>();
        services.AddTransient<AboutWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 清理资源
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

        base.OnExit(e);
    }
}
