using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using System.IO;
using System.Text.Json;

namespace SnowbreakToolbox.Services;

public class HistoryService : ISnowbreakHistory
{
    public static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private Dictionary<string, List<GachaItem>>? _gachaHistory;

    public Dictionary<string, List<GachaItem>> GetGachaHistory()
    {
        try
        {
            if (_gachaHistory == null)
            {
                if (!Directory.Exists(Global.UserPaths.DataPath))
                {
                    Directory.CreateDirectory(Global.UserPaths.DataPath);
                }

                if (!File.Exists(Global.UserPaths.DataFile))
                {
                    _gachaHistory = [];
                    SaveGachaHistory(_gachaHistory);
                }
                else
                {
                    var jsonString = File.ReadAllText(Global.UserPaths.DataFile);
                    jsonString = jsonString.Replace("Hisory", "History");   // Fix typo in previous version
                    jsonString = jsonString.Replace("ID", "Id");            // Fix different case for property 'Id' in ver1.x
                    _gachaHistory = JsonSerializer.Deserialize<Dictionary<string, List<GachaItem>>>(jsonString);
                    FixIDInVersion200Alpha12(_gachaHistory!);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "读取本地记录失败，使用空白记录");
            _gachaHistory = [];
        }

        return _gachaHistory!;
    }

    public static void FixIDInVersion200Alpha12(Dictionary<string, List<GachaItem>> history)
    {
        if (history.Count == 0) return;

        foreach (var (_, value) in history)
        {
            foreach (var item in value)
            {
                item.Id = item.Id.Replace("-", "").Replace(":", "").Replace(" ", "");
            }
        }
    }

    public void SaveGachaHistory(Dictionary<string, List<GachaItem>> newHistory)
    {
        var newHistoryString = JsonSerializer.Serialize(newHistory, _jsonOptions);

        if (File.Exists(Global.UserPaths.DataFile))
        {
            var oldHistoryString = File.ReadAllText(Global.UserPaths.DataFile);
            if (oldHistoryString == newHistoryString)
            {
                return;
            }
            if (oldHistoryString != "{}" && oldHistoryString!= "{\r\n  \"SpecialCharacterHistory\": [],\r\n  \"SpecialWeaponHistory\": [],\r\n  \"SpecialCharacterHistoryMihoyo\": [],\r\n  \"SpecialWeaponHistoryMihoyo\": [],\r\n  \"CommonCharacterHistory\": [],\r\n  \"CommonWeaponHistory\": []\r\n}")
                File.Copy(Global.UserPaths.DataFile, Global.UserPaths.DataPath + "\\" + DateTime.Now.ToString("yyyyMMddtthhmmss") + "_Backup" + ".json");
        }

        File.WriteAllText(Global.UserPaths.DataFile, newHistoryString);
    }
}
