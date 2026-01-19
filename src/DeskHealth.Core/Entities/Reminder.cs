namespace DeskHealth.Core.Entities;

/// <summary>
/// 提醒实体
/// </summary>
public class Reminder
{
    /// <summary>
    /// 提醒类型
    /// </summary>
    public ReminderType Type { get; set; }

    /// <summary>
    /// 提醒标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 提醒消息内容
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 提醒间隔（分钟）
    /// </summary>
    public int IntervalMinutes { get; set; }

    /// <summary>
    /// 自动关闭时间（秒）
    /// </summary>
    public int AutoCloseSeconds { get; set; }

    /// <summary>
    /// 图标/Emoji
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}
