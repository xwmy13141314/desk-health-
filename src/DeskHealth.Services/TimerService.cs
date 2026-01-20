using System.Diagnostics;
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
    private readonly List<ReminderType> _pendingReminders;
    private Timer? _mergeCheckTimer;
    private readonly object _lockObject = new();

    public event EventHandler<ReminderEventArgs>? ReminderTriggered;
    public event EventHandler<TimerState>? StateChanged;

    public TimerState State => _state;

    public TimerService(IConfigService configService)
    {
        _configService = configService;
        _timers = new Dictionary<ReminderType, Timer>();
        _state = TimerState.Running;
        _pendingReminders = new List<ReminderType>();
    }

    public void Start()
    {
        Debug.WriteLine("[TimerService] Start() called");

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

        Debug.WriteLine($"[TimerService] Loading config with {config.Reminders.Count} reminders");

        foreach (var reminder in config.Reminders)
        {
            var dueTime = TimeSpan.FromMinutes(reminder.IntervalMinutes);
            var period = TimeSpan.FromMinutes(reminder.IntervalMinutes);

            Debug.WriteLine($"[TimerService] Creating timer for {reminder.Type}: {reminder.Title} (interval: {reminder.IntervalMinutes} minutes)");

            var timer = new Timer(
                state => OnReminderTriggered(reminder.Type),
                null,
                dueTime,
                period);

            _timers[reminder.Type] = timer;
        }

        Debug.WriteLine($"[TimerService] Initialized {_timers.Count} timers");
    }

    private void OnReminderTriggered(ReminderType type)
    {
        Debug.WriteLine($"[TimerService] OnReminderTriggered called for {type}, state: {_state}");

        if (_state != TimerState.Running)
        {
            Debug.WriteLine($"[TimerService] Skipping reminder - not running (state: {_state})");
            return;
        }

        lock (_lockObject)
        {
            // 将触发类型添加到待处理列表
            _pendingReminders.Add(type);
            Debug.WriteLine($"[TimerService] Added {type} to pending list, count: {_pendingReminders.Count}");

            // 停止之前的合并检查定时器
            _mergeCheckTimer?.Dispose();

            // 启动新的合并检查定时器（500ms 延迟）
            _mergeCheckTimer = new Timer(
                _ => ProcessPendingReminders(),
                null,
                500,
                Timeout.Infinite);
        }
    }

    private void ProcessPendingReminders()
    {
        List<ReminderType> remindersToProcess;

        lock (_lockObject)
        {
            if (_pendingReminders.Count == 0)
            {
                Debug.WriteLine("[TimerService] No pending reminders to process");
                return;
            }

            // 复制待处理列表并清空
            remindersToProcess = _pendingReminders.ToList();
            _pendingReminders.Clear();
            _mergeCheckTimer?.Dispose();
            _mergeCheckTimer = null;
        }

        Debug.WriteLine($"[TimerService] Processing {remindersToProcess.Count} reminder(s): {string.Join(", ", remindersToProcess)}");

        try
        {
            if (remindersToProcess.Count > 1)
            {
                // 多个提醒同时触发，创建合并提醒
                var reminders = remindersToProcess
                    .Select(t => _configService.GetReminder(t))
                    .Where(r => r != null)
                    .ToArray()!;

                if (reminders.Length > 0)
                {
                    var combinedReminder = CombinedReminder.Create(reminders);
                    Debug.WriteLine($"[TimerService] Triggering COMBINED reminder: {combinedReminder.Title}");
                    ReminderTriggered?.Invoke(this, new ReminderEventArgs(combinedReminder));
                }
            }
            else
            {
                // 单个提醒
                var type = remindersToProcess[0];
                var reminder = _configService.GetReminder(type);
                if (reminder != null)
                {
                    Debug.WriteLine($"[TimerService] Triggering single reminder: {reminder.Title}");
                    ReminderTriggered?.Invoke(this, new ReminderEventArgs(reminder));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TimerService] ERROR in ProcessPendingReminders: {ex.Message}");
        }
    }

    public void Dispose()
    {
        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }
        _timers.Clear();

        _mergeCheckTimer?.Dispose();
        _mergeCheckTimer = null;

        _pauseCts?.Cancel();
        _pauseCts?.Dispose();
    }
}
