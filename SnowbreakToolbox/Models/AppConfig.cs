using System.ComponentModel;
using System.Drawing;
using System.Security.Cryptography.Pkcs;

namespace SnowbreakToolbox.Models;

public enum GamePlatform
{
    [Description("Default")]
    Default,
    [Description("Steam")]
    Steam
}

[Serializable]
public class AppConfig
{
    // General settings
    public string GamePath { get; set; } = string.Empty;
    public string UserPreferTheme { get; set; } = "Dark";
    public string LauncherExeFileName { get; set; } = "snow_launcher.exe";
    //public string GameWindowTitle { get; set; } = "Snowbreak: Containment Zone";
    public string GameWindowTitle { get; set; } = "屏幕截图 2023-10-08 121726.png ‎- 照片";
    public string GameWindowTitleCN { get; set; } = "尘白禁区";
    public string LauncherWindowTitle { get; set; } = "SnowBreak";
    public string GameSteamId { get; set; } = "2668080";
    public bool CloseLauncherWhenGameExit { get; set; } = false;
    public bool RunGameOnStart { get; set; } = false;
    public GamePlatform GamePlatform { get; set; } = GamePlatform.Default;

    // Reference settings
    public int LauncherStartBtnPosX { get; private set; } = 1085;
    public int LauncherStartBtnPosY { get; private set; } = 670;
    public int ReferenceScreenWidth { get; private set; } = 1920;
    public int ReferenceScreenHeight { get; private set; } = 1080;
    public int ReferenceLogBoxX0 { get; private set; } = 325;
    public int ReferenceLogBoxY0 { get; private set; } = 188;
    public int ReferenceLogBoxWidth { get; private set; } = 1260;
    public int ReferenceLogBoxHeight { get; private set; } = 680;
    public int ReferenceRareColorPosX { get; private set; } = 27;
    public int ReferenceNextPageArrowX { get; private set; } = 1665;
    public int ReferenceNextPageArrowY { get; private set; } = 600;

    // Pixel settings are all based on 1920x1080 resolution(reference settings above), calculate scale rate after get user real resolution
    // Client screen scale settings (for launcher)
    public double ClientScreenScale { get; set; } = 1;
    public int ClientScreenWidth { get; set; } = 1920;
    public int ClientScreenHeight { get; set; } = 1080;

    // Client game scale settings
    public double ClientGameScale { get; set; } = 1;
    public double ClientGameWidth { get; set; } = 1920;
    public double ClientGameHeight { get; set; } = 1080;
    public int ClientLogBoxX0 { get; set; } = 325;
    public int ClientLogBoxY0 { get; set; } = 188;
    public int ClientLogBoxWidth { get; set; } = 1260;      // History region box width
    public int ClientLogBoxHeight { get; set; } = 680;      // History region box height
    public int ClientRareColorPosX { get; set; } = 27;      // The rare color position X according to left border of log box
    public int ClientNextPageArrowX { get; set; } = 1665;
    public int ClientNextPageArrowY { get; set; } = 600;

    // Rare colors
    public string RareBlueColor { get; set; } = "#3763f2";
    public string RarePurpleColor { get; set; } = "#c069d6";
    public string RareGoldColor { get; set; } = "#e99b37";
}
