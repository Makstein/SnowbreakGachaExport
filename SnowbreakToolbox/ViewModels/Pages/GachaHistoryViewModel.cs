using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Services;
using SnowbreakToolbox.Tools;
using System.Collections.ObjectModel;
using System.Drawing;
using Vanara.PInvoke;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class GachaHistoryViewModel(ISnowbreakOcr snowbreakOcr, ISnowbreakConfig snowbreakConfig) : ObservableObject
{
    private readonly PaddleOrcService _paddleOrcService = (snowbreakOcr as PaddleOrcService)!;
    private readonly ISnowbreakConfig _configService = snowbreakConfig;

    public ObservableCollection<string> CCharHistory { get; private set; } = [];    // Common Character
    public ObservableCollection<string> SCharHistory { get; private set; } = [];    // Special Character
    public ObservableCollection<string> CWeaponHistory { get; private set; } = [];
    public ObservableCollection<string> SWeaponHistory { get; private set; } = [];

    [RelayCommand]
    private void OnGetHistory()
    {
        var config = _configService.GetConfig();

        var gameWindowHwnd = User32.FindWindow(null, config.GameWindowTitle);
        if (gameWindowHwnd == HWND.NULL) 
            gameWindowHwnd = User32.FindWindow(null, config.GameWindowTitleCN);
        if (gameWindowHwnd == HWND.NULL)
        {
            throw new Exception("刷新时无法找到游戏窗口");
        }

        User32.BringWindowToTop(gameWindowHwnd);

        Task.Delay(100).Wait();

        User32.GetWindowRect(gameWindowHwnd, out var rect);
        var gameWindowWidth = rect.Width;
        var gameWindowHeight = rect.Height;

        // re-calculate game scale
        if (gameWindowWidth != config.ClientGameWidth || gameWindowHeight != config.ClientGameHeight)
        {
            config.ClientGameScale = (double)gameWindowWidth / config.ReferenceScreenWidth;
            if (config.ReferenceScreenHeight * config.ClientGameScale != gameWindowHeight)
            {
                throw new Exception("游戏非16: 9分辨率");
            }

            config.ClientGameWidth = gameWindowWidth;
            config.ClientGameHeight = gameWindowHeight;
            config.ClientLogBoxX0 = (int)(config.ReferenceLogBoxX0 * config.ClientGameScale);
            config.ClientLogBoxY0 = (int)(config.ReferenceLogBoxY0 * config.ClientGameScale);
            config.ClientLogBoxWidth = (int)(config.ReferenceLogBoxWidth * config.ClientGameScale);
            config.ClientLogBoxHeight = (int)(config.ReferenceLogBoxHeight * config.ClientGameScale);

            App.GetService<ISnowbreakConfig>()?.SetConfig(config);
        }

        var image = 
            ScreenOperations.CaptureRegion(config.ClientLogBoxX0, config.ClientLogBoxY0, config.ClientLogBoxWidth, config.ClientLogBoxHeight);
        _paddleOrcService.GetText(image);

        User32.ShowWindow(gameWindowHwnd, ShowWindowCommand.SW_HIDE);
    }
}
