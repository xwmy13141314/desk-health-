using DeskHealth.Core.Entities;

namespace DeskHealth.Core.Interfaces;

/// <summary>
/// 提醒显示服务接口
/// </summary>
public interface INotifyService
{
    /// <summary>
    /// 显示提醒窗口
    /// </summary>
    /// <param name="reminder">提醒内容</param>
    void ShowReminder(Reminder reminder);

    /// <summary>
    /// 隐藏提醒窗口
    /// </summary>
    void HideReminder();

    /// <summary>
    /// 初始化服务
    /// </summary>
    void Initialize();
}
