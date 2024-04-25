using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SnowbreakToolbox.Services;

public class HistoryService : ISnowbreakHistory
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
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
                    _gachaHistory = JsonSerializer.Deserialize<Dictionary<string, List<GachaItem>>>(jsonString);
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

            File.Copy(Global.UserPaths.DataFile, Global.UserPaths.DataFile + "_Backup" + DateTime.Now.ToString("yyyyMMddhhmmss"));
        }

        File.WriteAllText(Global.UserPaths.DataFile, newHistoryString);
    }
}
