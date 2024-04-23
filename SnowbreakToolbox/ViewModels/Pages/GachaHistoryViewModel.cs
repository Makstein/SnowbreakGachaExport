using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using SnowbreakToolbox.Services;
using SnowbreakToolbox.Tools;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;
using Vanara.PInvoke;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.ViewModels.Pages;

public class DisplayItem
{
    public string ArmorName { get; set; }
    public string CharacterName { get; set; }
    public int Count { get; set; }              // Count since last 5 star character

    public DisplayItem(string armorName, string characterName, int count)
    {
        ArmorName = armorName;
        CharacterName = characterName;
        Count = count;
    }
}

public partial class GachaHistoryViewModel(ISnowbreakOcr snowbreakOcr, ISnowbreakConfig snowbreakConfig) : ObservableObject, INavigationAware, IDisposable
{
    private readonly PaddleOrcService _paddleOrcService = (snowbreakOcr as PaddleOrcService)!;
    private readonly ISnowbreakConfig _configService = snowbreakConfig;

    private AppConfig? _config;

    public int SelectedBannerIndex { get; set; }

    public List<GachaItem> CCharHistory { get; private set; } = [];                     // Common Character
    public List<GachaItem> SCharHistory { get; private set; } = [];                     // Special Character
    public List<GachaItem> CWeaponHistory { get; private set; } = [];
    public List<GachaItem> SWeaponHistory { get; private set; } = [];

    public ObservableCollection<DisplayItem> DisplayCCharHistory { get; private set; } = [new DisplayItem("TestArmor", "TestCharacter", 99)];            // Collection for display in page(to show count)
    public ObservableCollection<DisplayItem> DisplaySCharHistory { get; private set; } = [];
    public ObservableCollection<DisplayItem> DisplayCWeaponHistory { get; private set; } = [];
    public ObservableCollection<DisplayItem> DisplaySWeaponHistory { get; private set; } = [];

    [RelayCommand]
    private void OnGetHistory()
    {
        var gameWindowHwnd = User32.FindWindow(null, _config!.GameWindowTitle);
        if (gameWindowHwnd == HWND.NULL)
            gameWindowHwnd = User32.FindWindow(null, _config.GameWindowTitleCN);
        if (gameWindowHwnd == HWND.NULL)
        {
            throw new Exception("Exception: OnGetHistory() can't find game window");
        }

        User32.BringWindowToTop(gameWindowHwnd);

        Task.Delay(100).Wait();

        var image = 
            ScreenOperations.CaptureRegion(_config.ClientLogBoxX0, _config.ClientLogBoxY0, _config.ClientLogBoxWidth, _config.ClientLogBoxHeight);
        _paddleOrcService.GetText(image);

        var regions = _paddleOrcService.GetRegions(image);
        
        var list = new List<GachaItem>();
        foreach (var region in regions)
        {
            var color = image.GetPixel(_config.ClientRareColorPosX, (int)region[0].Rect.Center.Y);
            var rare = GetRare(color);
            var item = new GachaItem(region[0].Text, region[2].Text, region[1].Text == "武器" ? ItemType.Weapon : ItemType.Character, rare);
            list.Add(item);
        }
        switch (SelectedBannerIndex)
        {
            
        }

        User32.ShowWindow(gameWindowHwnd, ShowWindowCommand.SW_HIDE);
    }

    private int GetRare(Color color)
    {
        var blueMse = ColorMse(ColorTranslator.FromHtml(_config!.RareBlueColor), color);
        var purpleMse = ColorMse(ColorTranslator.FromHtml(_config.RarePurpleColor), color);
        var goldMse = ColorMse(ColorTranslator.FromHtml(_config.RareGoldColor), color);

        if (Math.Min(blueMse, Math.Min(purpleMse, goldMse)) >= 50) return 0;

        if (goldMse < purpleMse && goldMse < blueMse) return 5;
        if (purpleMse < goldMse && purpleMse < blueMse) return 4;
        return 3;
    }

    private static double ColorMse(Color a, Color b) => Vector3.Distance(new Vector3(a.R, a.G, a.B), new Vector3(b.R, b.G, b.B));

    public void OnNavigatedTo()
    {
        _config = _configService.GetConfig();
    }

    public void OnNavigatedFrom()
    {
        
    }

    public void Dispose()
    {
        
    }
}
