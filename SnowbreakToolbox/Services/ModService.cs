using System.IO;
using System.Text.Json;
using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;

namespace SnowbreakToolbox.Services;

public class ModService : IModService
{
    private static ModConfig? _modConfig;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public ModConfig GetModConfig()
    {
        try
        {
            if (_modConfig == null)
            {
                if (!Directory.Exists(Global.UserPaths.ConfPath)) { Directory.CreateDirectory(Global.UserPaths.ConfPath); }
            
                // Get latest character code file
                if (!File.Exists(Global.UserPaths.CharacterCodeFile)) throw new Exception("未找到角色代码文件 CharacterCode.json");
                var charCodeStr = File.ReadAllText(Global.UserPaths.CharacterCodeFile);
                var charCodes = JsonSerializer.Deserialize<List<Character>>(charCodeStr);
                if (charCodes == null) throw new Exception("角色代码文件转换失败");
            
                // Not have a mod config file yet
                if (!File.Exists(Global.UserPaths.ModConfFile))
                {
                    _modConfig = new ModConfig
                    {
                        Characters = charCodes
                    };
                }
                // Have a mod config file
                else
                {
                    var modConfigStr = File.ReadAllText(Global.UserPaths.ModConfFile);
                    _modConfig = JsonSerializer.Deserialize<ModConfig>(modConfigStr);
                    if (_modConfig == null)
                    {
                        var modConfigBackup = Path.ChangeExtension(Global.UserPaths.ModConfFile, ".backup.json");
                        File.Move(Global.UserPaths.ModConfFile, modConfigBackup);
                        _modConfig = new ModConfig();
                        throw new Exception("现有Mod配置文件读取失败，使用默认空配置");
                    }

                    // Have new characters
                    if (_modConfig.Characters.Count < charCodes.Count)
                    {
                        for (var i = _modConfig.Characters.Count - 1; i < charCodes.Count; i++)
                        {
                            _modConfig.Characters.Add(charCodes[i]);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("加载Mod信息失败 " + ex.Message, ex);
            var msgBox = new Wpf.Ui.Controls.MessageBox()
            {
                Title = "警告",
                MinWidth = 120,
                Content = "加载Mod信息失败 " + ex.Message,
                CloseButtonText = "确定",
                IsPrimaryButtonEnabled = false
            };

            _ = msgBox.ShowDialogAsync();
        }

        return _modConfig!;
    }

    public async Task<ModConfig> GetModConfigAsync()
    {
        return await Task.Run(GetModConfig);
    }

    public void Save()
    {
        if (_modConfig == null) return;
        File.WriteAllText(Global.UserPaths.ModConfFile, JsonSerializer.Serialize(_modConfig, JsonOptions));
    }
}