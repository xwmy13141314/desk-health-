namespace DeskHealth.Core.Events;

/// <summary>
/// 提醒事件参数
/// </summary>
public class ReminderEventArgs : EventArgs
{
    /// <summary>
    /// 提醒实体
    /// </summary>
    public Entities.Reminder Reminder { get; }

    /// <summary>
    /// 触发时间
    /// </summary>
    public DateTime TriggeredAt { get; }

    public ReminderEventArgs(Entities.Reminder reminder)
    {
        Reminder = reminder;
        TriggeredAt = DateTime.Now;
    }
}
