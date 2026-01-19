namespace DeskHealth.Core.Entities;

/// <summary>
/// 计时器状态枚举
/// </summary>
public enum TimerState
{
    /// <summary>
    /// 正常运行
    /// </summary>
    Running = 0,

    /// <summary>
    /// 暂停1小时
    /// </summary>
    Paused1H = 1,

    /// <summary>
    /// 暂停2小时
    /// </summary>
    Paused2H = 2
}
