using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using System.IO;
using System.Text.Json;

namespace SnowbreakToolbox.Services;

class ConfigService : ISnowbreakConfig
{
    private static AppConfig? _config;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Lazy load config instance
    /// </summary>
    /// <returns></returns>
    public AppConfig GetConfig()
    {
        try
        {
            if (_config == null)
            {
                if (!Directory.Exists(Global.UserPaths.ConfPath)) { Directory.CreateDirectory(Global.UserPaths.ConfPath); }

                if (!File.Exists(Global.UserPaths.ConfFile))
                {
                    _config = new AppConfig();
                }
                else
                {
                    var configStr = File.ReadAllText(Global.UserPaths.ConfFile);
                    _config = JsonSerializer.Deserialize<AppConfig>(configStr);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "读取配置文件失败，使用默认配置");
            _config = new AppConfig();
        }

        return _config!;
    }

    public void SetConfig(AppConfig config)
    {
        _config = config;
        Save();
    }

    public void Save()
    {
        File.WriteAllText(Global.UserPaths.ConfFile, JsonSerializer.Serialize(_config, _jsonOptions));
    }
}
