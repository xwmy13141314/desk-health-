namespace DeskHealth.Core.Entities;

/// <summary>
/// 提醒类型枚举
/// </summary>
public enum ReminderType
{
    /// <summary>
    /// 喝水提醒
    /// </summary>
    Hydration = 1,

    /// <summary>
    /// 休息提醒
    /// </summary>
    Break = 2,

    /// <summary>
    /// 组合提醒（喝水+休息）
    /// </summary>
    Combined = 3
}
