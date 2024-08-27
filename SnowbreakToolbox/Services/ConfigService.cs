using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using System.IO;
using System.Text.Json;
using Vanara.PInvoke;

namespace SnowbreakToolbox.Services;

public class ConfigService : ISnowbreakConfig
{
    private static readonly JsonSerializerOptions JsonOptions = new()
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
                    var charCodeStr = File.ReadAllText(Global.UserPaths.CharacterCodeFile);
                    _config = JsonSerializer.Deserialize<AppConfig>(configStr);
                    if (_config == null) throw new Exception("配置文件转换失败");

                    var charCodes = JsonSerializer.Deserialize<List<Character>>(charCodeStr);
                    if (charCodes == null) throw new Exception("角色代码文件转换失败");

                    // Have new characters
                    if (_config.Characters.Count < charCodes.Count)
                    {
                        for (var i = _config.Characters.Count - 1; i < charCodes.Count; i++)
                        {
                            _config.Characters.Add(charCodes[i]);
                        }
                    }
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
        File.WriteAllText(Global.UserPaths.ConfFile, JsonSerializer.Serialize(_config, JsonOptions));
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
        try
        {
            var curClientScreenWidth = User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN);
            var curClientScreenHeight = User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);

            if (config.ClientScreenWidth == curClientScreenWidth && config.ClientScreenHeight == curClientScreenHeight)
            {
                Log.Information("检测分辨率缩放成功，与配置文件相同，当前客户端系统分辨率：{ClientScreenWidth} x {ClientScreenHeight}",
                    config.ClientScreenWidth,
                    config.ClientScreenHeight);
                return;
            }

            config.ClientScreenScale = (double)curClientScreenWidth / config.ReferenceScreenWidth;
            if (Math.Abs(config.ReferenceScreenHeight * config.ClientScreenScale - curClientScreenHeight) > 0.01)
            {
                throw new Exception("系统分辨率缩放初始化失败: 非16: 9分辨率");
            }

            config.ClientScreenWidth = curClientScreenWidth;
            config.ClientScreenHeight = curClientScreenHeight;

            Log.Information("检测并记录分辨率缩放成功，当前客户端系统分辨率：{ClientScreenWidth} x {ClientScreenHeight}",
                config.ClientScreenWidth,
                config.ClientScreenHeight);
        }
        catch (Exception ex)
        {
            var msgBox = new Wpf.Ui.Controls.MessageBox()
            {
                Title = "错误",
                Content = ex.Message,
                CloseButtonText = "确定"
            };
            msgBox.ShowDialogAsync();
        }
    }
}