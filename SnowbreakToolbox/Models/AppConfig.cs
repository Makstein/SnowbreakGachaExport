namespace SnowbreakToolbox.Models;

[Serializable]
public class AppConfig
{
    // General settings
    public string GamePath { get; set; } = string.Empty;
    public string UserPreferTheme { get; set; } = "Dark";
    public string LauncherExeFileName { get; set; } = "snow_launcher.exe";
    public string GameWindowTitle { get; set; } = "Snowbreak: Containment Zone";
    public string GameWindowTitleCN { get; set; } = "";
    public string LauncherWindowTitle { get; set; } = "SnowBreak";
    public bool CloseLauncherWhenGameExit { get; set; } = false;

    // Pixel settings are all based on 1920x1080 resolution, calculate scale rate after get user real resolution
    // Client specific settings
    public int ReferenceScreenWidth { get; set; } = 1920;
    public int ReferenceScreenHeight { get; set; } = 1080;
    public int ClientScreenWidth { get; set; } = 1920;
    public int ClientScreenHeight { get; set; } = 1080;
    public double ClientScale { get; set; } = 1;

    // Game specific settings
    public int LauncherStartBtnPosX { get; set; } = 1400; // 1085
    public int LauncherStartBtnPosY { get; set; } = 810; // 670
}
