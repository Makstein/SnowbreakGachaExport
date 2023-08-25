using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia.Extensions.Controls;
using Newtonsoft.Json;
using SnowbreakGachaExport.Models;
using SnowbreakGachaExport.Models.Global;

namespace SnowbreakGachaExport.Tools;

public static class JsonOperate
{
    public static Dictionary<string, List<HistoryItem>>? Read()
    {
        Directory.CreateDirectory("./Data");
        var path = Path.Combine(UserPaths.DataPath, UserPaths.GachaJsonName);
        if (!File.Exists(path))
        {
            File.Create(path);
        }

        var jsonString = File.ReadAllText(path, Encoding.Default);
        return JsonConvert.DeserializeObject<Dictionary<string, List<HistoryItem>>>(jsonString);
    }

    public static void Save(Dictionary<string, List<HistoryItem>> dictionary)
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
}