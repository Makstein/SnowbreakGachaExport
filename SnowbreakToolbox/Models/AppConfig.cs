namespace SnowbreakToolbox.Models;

[Serializable]
public class AppConfig
{
    public string GamePath { get; set; } = string.Empty;
    public string UserPreferTheme { get; set; } = "Light";
}
