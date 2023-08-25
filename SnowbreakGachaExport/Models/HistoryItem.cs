using Newtonsoft.Json;

namespace SnowbreakGachaExport.Models;

public enum ItemType
{
    Character,
    Weapon,
}

public class HistoryItem
{
    public string ID { get; }
    public string Name { get; }
    public ItemType Type { get; }
    public int Star { get; }
    public string Time { get; }

    public HistoryItem(string name = "", string time = "", ItemType type = ItemType.Weapon, int star = 4)
    {
        Name = name;
        Type = type;
        Star = star;
        Time = time;
        ID = name + time;
    }
    
    [JsonConstructor]
    public HistoryItem(string id, string name = "", string time = "", ItemType type = ItemType.Weapon, int star = 4)
    {
        Name = name;
        Type = type;
        Star = star;
        Time = time;
        ID = id;
    }
}