namespace SnowbreakGachaExport.Models;

public class FiveStarItem
{
    public string Name { get; }
    public int Count { get; }

    public FiveStarItem(string name = "", int count = 0)
    {
        Name = name;
        Count = count;
    }
}