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
                    if (_modConfig == null) throw new Exception("Mod配置文件转换失败");

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
            Log.Error("读取Mod信息失败 " + ex.Message, ex);
        }

        return _modConfig!;
    }

    public void Save()
    {
        File.WriteAllText(Global.UserPaths.ModConfFile, JsonSerializer.Serialize(_modConfig, JsonOptions));
    }
}