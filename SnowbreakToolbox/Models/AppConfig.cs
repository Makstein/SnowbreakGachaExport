using System.Security.Cryptography.Pkcs;

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

    // Reference settings
    public int LauncherStartBtnPosX { get; set; } = 1085;
    public int LauncherStartBtnPosY { get; set; } = 670;
    public int ReferenceScreenWidth { get; set; } = 1920;
    public int ReferenceScreenHeight { get; set; } = 1080;
    public int ReferenceLogBoxX0 { get; set; } = 280;
    public int ReferenceLogBoxY0 { get; set; } = 188;
    public int ReferenceLogBoxWidth { get; set; } = 1260;
    public int ReferenceLogBoxHeight { get; set; } = 680;

    // Pixel settings are all based on 1920x1080 resolution(reference settings above), calculate scale rate after get user real resolution
    // Client screen scale settings (for launcher)
    public double ClientScreenScale { get; set; } = 1;
    public int ClientScreenWidth { get; set; } = 1920;
    public int ClientScreenHeight { get; set; } = 1080;

    // Client game scale settings
    public double ClientGameScale { get; set; } = 1;
    public double ClientGameWidth { get; set; } = 1920;
    public double ClientGameHeight { get; set; } = 1080;
    public int ClientLogBoxX0 { get; set; } = 280;
    public int ClientLogBoxY0 { get; set; } = 188;
    public int ClientLogBoxWidth { get; set; } = 1260;
    public int ClientLogBoxHeight { get; set; } = 680;
}
