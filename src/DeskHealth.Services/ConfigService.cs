using System.Text.Json;
using DeskHealth.Core.Entities;
using DeskHealth.Core.Interfaces;

namespace DeskHealth.Services;

/// <summary>
/// 配置服务实现
/// </summary>
public class ConfigService : IConfigService
{
    private readonly string _configPath;
    private AppConfig? _cachedConfig;

    public string ConfigPath => _configPath;

    public ConfigService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "DeskHealth");

        // 确保目录存在
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        _configPath = Path.Combine(appFolder, "config.json");
    }

    public AppConfig LoadConfig()
    {
        if (_cachedConfig != null)
        {
            return _cachedConfig;
        }

        if (!File.Exists(_configPath))
        {
            _cachedConfig = GetDefaultConfig();
            SaveConfig(_cachedConfig);
            return _cachedConfig;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            _cachedConfig = JsonSerializer.Deserialize<AppConfig>(json) ?? GetDefaultConfig();
            return _cachedConfig;
        }
        catch
        {
            // 如果读取失败，返回默认配置
            _cachedConfig = GetDefaultConfig();
            return _cachedConfig;
        }
    }

    public void SaveConfig(AppConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(_configPath, json);
            _cachedConfig = config;
        }
        catch
        {
            // 静默处理保存失败
        }
    }

    public Reminder GetReminder(ReminderType type)
    {
        var config = LoadConfig();
        var reminder = config.Reminders.FirstOrDefault(r => r.Type == type);

        if (reminder == null)
        {
            // 返回默认提醒
            return type == ReminderType.Hydration
                ? new Reminder
                {
                    Type = ReminderType.Hydration,
                    Title = "该喝水了 🥤",
                    Message = "保持健康，多喝水！",
                    IntervalMinutes = 30,
                    AutoCloseSeconds = 10,
                    Icon = "🥤"
                }
                : new Reminder
                {
                    Type = ReminderType.Break,
                    Title = "休息一下 🌿",
                    Message = "站起来活动活动，保护颈椎！",
                    IntervalMinutes = 60,
                    AutoCloseSeconds = 10,
                    Icon = "🌿"
                };
        }

        return reminder;
    }

    /// <summary>
    /// 获取默认配置
    /// </summary>
    private static AppConfig GetDefaultConfig()
    {
        return new AppConfig
        {
            Version = "1.0.0",
            AutoStart = true,
            EnableSound = false,
            WindowOpacity = 0.7,
            WindowWidth = 200,
            WindowHeight = 80,
            Reminders = new List<Reminder>
            {
                new Reminder
                {
                    Type = ReminderType.Hydration,
                    Title = "该喝水了 🥤",
                    Message = "保持健康，多喝水！",
                    IntervalMinutes = 30,
                    AutoCloseSeconds = 10,
                    Icon = "🥤"
                },
                new Reminder
                {
                    Type = ReminderType.Break,
                    Title = "休息一下 🌿",
                    Message = "站起来活动活动，保护颈椎！",
                    IntervalMinutes = 60,
                    AutoCloseSeconds = 10,
                    Icon = "🌿"
                }
            }
        };
    }
}
