using DeskHealth.Core.Entities;

namespace DeskHealth.Core.Interfaces;

/// <summary>
/// 配置服务接口
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// 加载配置
    /// </summary>
    /// <returns>应用配置</returns>
    AppConfig LoadConfig();

    /// <summary>
    /// 保存配置
    /// </summary>
    /// <param name="config">配置对象</param>
    void SaveConfig(AppConfig config);

    /// <summary>
    /// 获取指定类型的提醒配置
    /// </summary>
    /// <param name="type">提醒类型</param>
    /// <returns>提醒配置</returns>
    Reminder GetReminder(ReminderType type);

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    string ConfigPath { get; }
}
