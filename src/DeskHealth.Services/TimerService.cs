using System.Threading;
using DeskHealth.Core.Entities;
using DeskHealth.Core.Events;
using DeskHealth.Core.Interfaces;

namespace DeskHealth.Services;

/// <summary>
/// 计时器服务实现
/// </summary>
public class TimerService : ITimerService, IDisposable
{
    private readonly Dictionary<ReminderType, Timer> _timers;
    private readonly IConfigService _configService;
    private TimerState _state;
    private CancellationTokenSource? _pauseCts;

    public event EventHandler<ReminderEventArgs>? ReminderTriggered;
    public event EventHandler<TimerState>? StateChanged;

    public TimerState State => _state;

    public TimerService(IConfigService configService)
    {
        _configService = configService;
        _timers = new Dictionary<ReminderType, Timer>();
        _state = TimerState.Running;
    }

    public void Start()
    {
        if (_state != TimerState.Running)
        {
            _state = TimerState.Running;
            StateChanged?.Invoke(this, _state);
        }

        InitializeTimers();
    }

    public void Pause(TimeSpan duration)
    {
        // 停止所有计时器
        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }
        _timers.Clear();

        // 更新状态
        var previousState = _state;
        _state = duration.TotalHours >= 2 ? TimerState.Paused2H : TimerState.Paused1H;
        StateChanged?.Invoke(this, _state);

        // 设置恢复计时器
        _pauseCts?.Cancel();
        _pauseCts = new CancellationTokenSource();

        Task.Delay(duration, _pauseCts.Token)
            .ContinueWith(_ =>
            {
                if (!_pauseCts.Token.IsCancellationRequested)
                {
                    Resume();
                }
            }, _pauseCts.Token);
    }

    public void Resume()
    {
        if (_state == TimerState.Running)
        {
            return;
        }

        // 取消暂停计时器
        _pauseCts?.Cancel();
        _pauseCts?.Dispose();
        _pauseCts = null;

        // 恢复状态
        _state = TimerState.Running;
        StateChanged?.Invoke(this, _state);

        // 重新初始化计时器
        InitializeTimers();
    }

    public void Stop()
    {
        // 停止所有计时器
        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }
        _timers.Clear();

        // 取消暂停
        _pauseCts?.Cancel();
        _pauseCts?.Dispose();
        _pauseCts = null;

        _state = TimerState.Running;
    }

    private void InitializeTimers()
    {
        // 清理现有计时器
        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }
        _timers.Clear();

        var config = _configService.LoadConfig();

        foreach (var reminder in config.Reminders)
        {
            var dueTime = TimeSpan.FromMinutes(reminder.IntervalMinutes);
            var period = TimeSpan.FromMinutes(reminder.IntervalMinutes);

            var timer = new Timer(
                state => OnReminderTriggered(reminder.Type),
                null,
                dueTime,
                period);

            _timers[reminder.Type] = timer;
        }
    }

    private void OnReminderTriggered(ReminderType type)
    {
        if (_state != TimerState.Running)
        {
            return;
        }

        try
        {
            var reminder = _configService.GetReminder(type);
            ReminderTriggered?.Invoke(this, new ReminderEventArgs(reminder));
        }
        catch
        {
            // 静默处理错误
        }
    }

    public void Dispose()
    {
        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }
        _timers.Clear();

        _pauseCts?.Cancel();
        _pauseCts?.Dispose();
    }
}
