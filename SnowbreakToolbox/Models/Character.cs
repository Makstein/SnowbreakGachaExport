namespace SnowbreakToolbox.Models;

public class Character
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Dictionary<string, string> ArmorCodeNames { get; set; } = new();

    public Character()
    {
    }

    public Character(string name, string code, Dictionary<string, string> armorCodeNames)
    {
        Name = name;
        Code = code;
        ArmorCodeNames = armorCodeNames;
    }
}