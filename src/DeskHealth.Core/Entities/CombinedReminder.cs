namespace DeskHealth.Core.Entities;

/// <summary>
/// 组合提醒实体，用于同时显示多个提醒
/// </summary>
public class CombinedReminder : Reminder
{
    /// <summary>
    /// 子提醒列表
    /// </summary>
    public List<Reminder> SubReminders { get; set; } = new();

    /// <summary>
    /// 创建组合提醒
    /// </summary>
    public static CombinedReminder Create(params Reminder[] reminders)
    {
        return new CombinedReminder
        {
            Type = ReminderType.Combined,
            Title = "健康提醒 🌟",
            Message = "该喝水了，起来活动一下吧！",
            Icon = "🌟",
            IntervalMinutes = reminders.Max(r => r.IntervalMinutes),
            AutoCloseSeconds = reminders.Max(r => r.AutoCloseSeconds),
            SubReminders = reminders.ToList()
        };
    }
}
