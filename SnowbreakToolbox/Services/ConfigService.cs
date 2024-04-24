using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using System.IO;
using System.Text.Json;
using Vanara.PInvoke;

namespace SnowbreakToolbox.Services;

public class ConfigService : ISnowbreakConfig
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static AppConfig? _config;

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

        // Re-calculate game scale every time call GetConfig()
        InitConfigResolutionScale(ref _config!);

        return _config!;
    }

    public void Save()
    {
        File.WriteAllText(Global.UserPaths.ConfFile, JsonSerializer.Serialize(_config, _jsonOptions));
    }

    /// <summary>
    /// Set config and save
    /// </summary>
    /// <param name="config"></param>
    public void SetConfig(AppConfig config)
    {
        _config = config;
        Save();
    }

    private void InitConfigResolutionScale(ref AppConfig config)
    {
        var curClientScreenWidth = User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN);
        var curClientScreenHeight = User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);

        if (config.ClientScreenWidth == curClientScreenWidth && config.ClientScreenHeight == curClientScreenHeight)
            return;

        config.ClientScreenScale = (double)curClientScreenWidth / config.ReferenceScreenWidth;
        if (config.ReferenceScreenHeight * config.ClientScreenScale != curClientScreenHeight)
        {
            Log.Error("分辨率缩放初始化失败: 非16: 9分辨率");
            throw new Exception("分辨率缩放初始化失败: 非16: 9分辨率");
        }

        config.ClientScreenWidth = curClientScreenWidth;
        config.ClientScreenHeight = curClientScreenHeight;

        Log.Information("初始化设置分辨路缩放成功，当前客户端分辨率：{ClientScreenWidth} x {ClientScreenHeight}",
            config.ClientScreenWidth,
            config.ClientScreenHeight);
    }
}