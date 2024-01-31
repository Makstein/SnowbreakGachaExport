using Avalonia.Extensions.Controls;
using Newtonsoft.Json;
using SnowbreakGachaExport.Models;
using SnowbreakGachaExport.Models.Global;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SnowbreakGachaExport.Tools;

public static class JsonOperate
{
    public static Dictionary<string, List<HistoryItem>> ReadHistory()
    {
        Directory.CreateDirectory("./Data");
        var path = Path.Combine(UserPaths.DataPath, UserPaths.GachaJsonName);
        if (!File.Exists(path))
        {
            File.Create(path);
        }

        var jsonString = File.ReadAllText(path, Encoding.Default);
        
        // Fix the typo last version
        jsonString = jsonString.Replace("CommonCharacterHisory", "CommonCharacterHistory");
        
        if (jsonString.Length == 0)
            return new Dictionary<string, List<HistoryItem>>()
        {
            { Resource.CommonCharacterHisoryName, new List<HistoryItem>() },
            { Resource.CommonWeaponHistoryName, new List<HistoryItem>() },
            { Resource.SpecialCharacterHistoryName, new List<HistoryItem>() },
            { Resource.SpecialWeaponHistoryName, new List<HistoryItem>() }
        };

        var res = JsonConvert.DeserializeObject<Dictionary<string, List<HistoryItem>>>(jsonString);
        return res ?? new Dictionary<string, List<HistoryItem>>()
        {
            { Resource.CommonCharacterHisoryName, new List<HistoryItem>() },
            { Resource.CommonWeaponHistoryName, new List<HistoryItem>() },
            { Resource.SpecialCharacterHistoryName, new List<HistoryItem>() },
            { Resource.SpecialWeaponHistoryName, new List<HistoryItem>() }
        };
    }

    public static void SaveHistory(Dictionary<string, List<HistoryItem>> dictionary)
    {
        Directory.CreateDirectory("./Data");
        var path = Path.Combine(UserPaths.DataPath, UserPaths.GachaJsonName);
        if (!File.Exists(path))
        {
            MessageBox.Show("错误", "缺少Json文件，无法保存");
            return;
        }

        // BackUp old json
        File.Copy(path, Path.Combine(UserPaths.DataPath
            , UserPaths.GachaJsonName.Replace(".json", "") + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json"));

        File.WriteAllText(path, JsonConvert.SerializeObject(dictionary, Formatting.Indented));
    }

    public static GameConfig ReadConfig()
    {
        var jsonString = File.ReadAllText(UserPaths.GameConfigFileName);
        var gameConfig = JsonConvert.DeserializeObject<GameConfig>(jsonString);

        Debug.WriteLine(gameConfig?.GameWindowTitle);

        return gameConfig;
    }

    public static void SaveConfig(ref GameConfig gameConfig)
    {
        var jsonString = JsonConvert.SerializeObject(gameConfig, Formatting.Indented);
        File.WriteAllText(UserPaths.GameConfigFileName, jsonString);
    }
}