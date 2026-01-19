using System.Text.Json.Serialization;

namespace DeskHealth.Core.Entities;

/// <summary>
/// 应用配置实体
/// </summary>
public class AppConfig
{
    /// <summary>
    /// 应用版本
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// 是否开机自启
    /// </summary>
    [JsonPropertyName("autoStart")]
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// 是否启用声音
    /// </summary>
    [JsonPropertyName("enableSound")]
    public bool EnableSound { get; set; } = false;

    /// <summary>
    /// 提醒配置列表
    /// </summary>
    [JsonPropertyName("reminders")]
    public List<Reminder> Reminders { get; set; } = new();

    /// <summary>
    /// 窗口透明度 (0.0 - 1.0)
    /// </summary>
    [JsonPropertyName("windowOpacity")]
    public double WindowOpacity { get; set; } = 0.7;

    /// <summary>
    /// 窗口宽度
    /// </summary>
    [JsonPropertyName("windowWidth")]
    public double WindowWidth { get; set; } = 200;

    /// <summary>
    /// 窗口高度
    /// </summary>
    [JsonPropertyName("windowHeight")]
    public double WindowHeight { get; set; } = 80;
}
