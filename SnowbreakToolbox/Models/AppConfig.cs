namespace SnowbreakToolbox.Models;

[Serializable]
public class AppConfig
{
    // General settings
    public string GamePath { get; set; } = string.Empty;
    public string UserPreferTheme { get; set; } = "Light";
    public string LauncherExeFileName { get; set; } = "snow_launcher.exe";
    public string GameWindowTitle { get; set; } = "Snowbreak: Container Zone";
    public string LauncherWindowTitle { get; set; } = "xxxxLauncher";

    // Pixel settings are all based on 1920x1080 resolution, calculate scale rate after get user real resolution
    // Client specific settings
    public int ClientScreenWidth { get; set; } = 1920;
    public int ClientScreenHeight { get; set; } = 1080;
    public double ClientScale { get; set; } = 1;

    // Game specific settings
    public int LauncherStartBtnPosX { get; set; } = 1420;
    public int LauncherStartBtnPosY { get; set; } = 820;
    
}
