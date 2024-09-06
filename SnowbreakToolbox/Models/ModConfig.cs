namespace SnowbreakToolbox.Models;

public class ModConfig
{
    public List<Character> Characters { get; init; } = [];
    
    public List<ModPakInfo> Mods { get; set; } = [];
}