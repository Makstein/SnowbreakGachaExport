namespace SnowbreakGachaExport.Models;

public enum ItemType
{
    Character,
    Weapon,
}

public class HistoryItem
{
    public string Name { get; }
    public ItemType Type { get; }
    public int Star { get; }
    public int Count { get; } = 0;

    public HistoryItem(string name = "", ItemType type = ItemType.Weapon, int star = 4)
    {
        Name = name;
        Type = type;
        Star = star;
    }
}