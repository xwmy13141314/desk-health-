using DeskHealth.Core.Entities;
using DeskHealth.Core.Events;

namespace DeskHealth.Core.Interfaces;

/// <summary>
/// 计时器服务接口
/// </summary>
public interface ITimerService
{
    /// <summary>
    /// 提醒触发事件
    /// </summary>
    event EventHandler<ReminderEventArgs>? ReminderTriggered;

    /// <summary>
    /// 状态变化事件
    /// </summary>
    event EventHandler<TimerState>? StateChanged;

    /// <summary>
    /// 当前计时器状态
    /// </summary>
    TimerState State { get; }

    /// <summary>
    /// 启动计时器
    /// </summary>
    void Start();

    /// <summary>
    /// 暂停提醒指定时长
    /// </summary>
    /// <param name="duration">暂停时长</param>
    void Pause(TimeSpan duration);

    /// <summary>
    /// 恢复提醒
    /// </summary>
    void Resume();

    /// <summary>
    /// 停止计时器
    /// </summary>
    void Stop();
}
