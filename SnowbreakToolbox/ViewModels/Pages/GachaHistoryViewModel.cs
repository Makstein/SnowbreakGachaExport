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

public partial class GachaHistoryViewModel(ISnowbreakOcr snowbreakOcr, ISnowbreakConfig snowbreakConfig, ISnowbreakHistory snowbreakHistory) : ObservableObject, INavigationAware, IDisposable
{
    private readonly PaddleOrcService _paddleOrcService = (snowbreakOcr as PaddleOrcService)!;
    private readonly ISnowbreakConfig _configService = snowbreakConfig;
    private readonly ISnowbreakHistory _historyService = snowbreakHistory;

    private AppConfig? _config;
    private bool _initialized;

    [ObservableProperty] private double _avgCc;     // Common character
    [ObservableProperty] private int _minCc;
    [ObservableProperty] private double _avgCw;     // Common weapon
    [ObservableProperty] private int _minCw;
    [ObservableProperty] private double _avgSc;     // Special character
    [ObservableProperty] private int _minSc;
    [ObservableProperty] private double _avgSw;     // Special weapon
    [ObservableProperty] private int _minSw;

    public int SelectedBannerIndex { get; set; }
    public int SelectedGachaMode { get; set; }                                          // 0: 59% 1:100%

    public List<GachaItem> CCharHistory { get; private set; } = [];                     // Common Character 50%
    public List<GachaItem> SCharHistory { get; private set; } = [];                     // Special Character 50%
    public List<GachaItem> CCharPreferenceHistory { get; private set; } = [];           // Common Character 100%
    public List<GachaItem> SCharPreferenceHistory { get; private set; } = [];           // Special Character 100%
    public List<GachaItem> CWeaponHistory { get; private set; } = [];
    public List<GachaItem> SWeaponHistory { get; private set; } = [];
    public List<GachaItem> CWeaponPreferenceHistory { get; private set; } = [];
    public List<GachaItem> SWeaponPreferenceHistory { get; private set; } = [];

    public ObservableCollection<DisplayItem> DisplayCCharHistory { get; private set; } = [];            // Collection for display in page(to show count)
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
            throw new Exception("Exception: can't find game window");
        }

        User32.GetWindowRect(gameWindowHwnd, out var rect);
        var gameWindowWidth = rect.Width;
        var gameWindowHeight = rect.Height;
        User32.BringWindowToTop(gameWindowHwnd);

        _config.ClientGameScale = (double)gameWindowWidth / _config.ReferenceScreenWidth;
        if (Math.Abs(_config.ReferenceScreenHeight * _config.ClientGameScale - gameWindowHeight) > 0.01)
        {
            throw new Exception("游戏分辨率初始化失败：非16: 9分辨率");
        }

        _config.ClientGameWidth = gameWindowWidth;
        _config.ClientGameHeight = gameWindowHeight;
        _config.ClientLogBoxX0 = (int)(_config.ReferenceLogBoxX0 * _config.ClientGameScale);
        _config.ClientLogBoxY0 = (int)(_config.ReferenceLogBoxY0 * _config.ClientGameScale);
        _config.ClientLogBoxWidth = (int)(_config.ReferenceLogBoxWidth * _config.ClientGameScale);
        _config.ClientLogBoxHeight = (int)(_config.ReferenceLogBoxHeight * _config.ClientGameScale);
        _config.ClientRareColorPosX = (int)(_config.ReferenceRareColorPosX * _config.ClientGameScale);
        _config.ClientNextPageArrowX = (int)(_config.ReferenceNextPageArrowX * _config.ClientGameScale);
        _config.ClientNextPageArrowY = (int)(_config.ReferenceNextPageArrowY * _config.ClientGameScale);

        App.GetService<ISnowbreakConfig>()?.SetConfig(_config);

        Task.Delay(200).Wait();

        var list = new List<GachaItem>();
        Bitmap? lastCapturedImage = null;
        while (true)
        {
            var image =
            ScreenOperations.CaptureRegion(_config.ClientLogBoxX0, _config.ClientLogBoxY0, _config.ClientLogBoxWidth, _config.ClientLogBoxHeight);

            if (lastCapturedImage != null)
            {
                var mse = ImageOperations.ImageMse(lastCapturedImage, image);
                if (mse < 45)
                {
                    break;
                }
            }

            lastCapturedImage = new(image);

            //_paddleOrcService.GetText(image);
            var regions = _paddleOrcService.GetRegions(image);

            foreach (var region in regions)
            {
                var color = image.GetPixel(_config.ClientRareColorPosX, (int)region[0].Rect.Center.Y);
                var rare = GetRare(color);
                var item = new GachaItem(region[0].Text, region[2].Text, region[1].Text == "武器" ? ItemType.Weapon : ItemType.Character, rare);
                list.Add(item);
            }

            MouseOperations.LeftMouseClick(_config.ClientNextPageArrowX, _config.ClientNextPageArrowY);

            Task.Delay(200).Wait();
        }

        MergeHistory(list);

        User32.ShowWindow(gameWindowHwnd, ShowWindowCommand.SW_MINIMIZE);
    }

    [RelayCommand]
    private void OnExportHistory()
    {

    }

    [RelayCommand]
    private void OnImportHistory()
    {

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

    private void MergeHistory(List<GachaItem> newHistory)
    {
        List<GachaItem> curHistory = [];

        switch (SelectedBannerIndex)
        {
            case 0:
                curHistory = SCharHistory;
                break;
            case 1:
                curHistory = SWeaponHistory;
                break;
            case 2:
                curHistory = CCharHistory;
                break;
            case 3:
                curHistory = CWeaponHistory;
                break;
        }

        var index = newHistory.Count;
        if (curHistory.Count > 0)
        {
            index = newHistory.FindIndex(x => x.Id == curHistory[0].Id);
            if (index == -1)
            {
                index = newHistory.Count;
            }
        }

        curHistory.InsertRange(0, newHistory.Take(index));

        UpdateDisplay(SelectedBannerIndex);
    }

    private void UpdateDisplay(int bannerIndex)
    {
        var count = 0;
        var curHistory = new List<GachaItem>();
        var curDisplayHistory = new ObservableCollection<DisplayItem>();

        switch (bannerIndex)
        {
            case 0:
                curHistory = SelectedGachaMode == 0 ? SCharHistory : SCharPreferenceHistory;
                curDisplayHistory = DisplaySCharHistory;
                break;
            case 1:
                curHistory = SelectedGachaMode == 0 ? SWeaponHistory : SWeaponPreferenceHistory;
                curDisplayHistory = DisplaySWeaponHistory;
                break;
            case 2:
                curHistory = SelectedGachaMode == 0 ? CCharHistory : CCharPreferenceHistory;
                curDisplayHistory = DisplayCCharHistory;
                break;
            case 3:
                curHistory = SelectedGachaMode == 0 ? CWeaponHistory : CWeaponPreferenceHistory;
                curDisplayHistory = DisplayCWeaponHistory;
                break;
        }

        curDisplayHistory.Clear();
        for (var i = curHistory.Count - 1; i >= 0; i--)
        {
            count++;
            if (curHistory[i].Star == 5)
            {
                if (curHistory[i].Type == ItemType.Character)
                {
                    var names = curHistory[i].Name.Split('-');
                    curDisplayHistory.Insert(0, new DisplayItem(names[0], names[1], count));
                }
                else
                {
                    string[] names = [curHistory[i].Name, "武器"];
                    curDisplayHistory.Insert(0, new DisplayItem(names[0], names[1], count));
                }
                count = 0;
            }
        }

        if (curDisplayHistory.Count <= 0)
        {
            return;
        }

        switch (bannerIndex)
        {
            case 0:
                AvgSc = curDisplayHistory.Average(x => x.Count);
                MinSc = curDisplayHistory.MinBy(x => x.Count)!.Count;
                break;
            case 1:
                AvgSw = curDisplayHistory.Average(x => x.Count);
                MinSw = curDisplayHistory.MinBy(x => x.Count)!.Count;
                break;
            case 2:
                AvgCc = curDisplayHistory.Average(x => x.Count);
                MinCc = curDisplayHistory.MinBy(x => x.Count)!.Count;
                break;
            case 3:
                AvgCw = curDisplayHistory.Average(x => x.Count);
                MinCw = curDisplayHistory.MinBy(x => x.Count)!.Count;
                break;
        }
    }

    private void InitializeViewModel()
    {
        if (!_initialized)
        {
            // Load local history cache
            var historyItems = _historyService.GetGachaHistory();
            CCharHistory = historyItems.TryGetValue(NameResource.CommonCharacterHistoryName, out List<GachaItem>? localCcHistory) ? localCcHistory : [];
            SCharHistory = historyItems.TryGetValue(NameResource.SpecialCharacterHistoryName, out List<GachaItem>? localScHistory) ? localScHistory : [];
            CWeaponHistory = historyItems.TryGetValue(NameResource.CommonWeaponHistoryName, out List<GachaItem>? localCwHistory) ? localCwHistory : [];
            SWeaponHistory = historyItems.TryGetValue(NameResource.SpecialWeaponHistoryName, out List<GachaItem>? localSwHistory) ? localSwHistory : [];
            CCharPreferenceHistory = historyItems.TryGetValue(NameResource.CommonCharacterPreferenceHistoryName, out List<GachaItem>? localCCPHistory) ? localCCPHistory : [];
            SCharPreferenceHistory = historyItems.TryGetValue(NameResource.SpecialCharacterPreferenceHistoryName, out List<GachaItem>? localSCPHistory) ? localSCPHistory : [];
            CWeaponPreferenceHistory = historyItems.TryGetValue(NameResource.CommonWeaponPreferenceHistoryName, out List<GachaItem>? localCWPHistory) ? localCWPHistory : [];
            SWeaponPreferenceHistory = historyItems.TryGetValue(NameResource.SpecialWeaponPreferenceHistoryName, out List<GachaItem>? localSWPHistory) ? localSWPHistory : [];

            UpdateDisplay(0);
            UpdateDisplay(1);
            UpdateDisplay(2);
            UpdateDisplay(3);

            _initialized = true;
        }
    }

    public void OnNavigatedTo()
    {
        _config = _configService.GetConfig();

        if (!_initialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {

    }

    public void Dispose()
    {
        if (!_initialized)
        {
            return;
        }

        Dictionary<string, List<GachaItem>> newHistory = [];
        newHistory.Add(NameResource.CommonCharacterHistoryName, CCharHistory);
        newHistory.Add(NameResource.SpecialCharacterHistoryName, SCharHistory);
        newHistory.Add(NameResource.CommonWeaponHistoryName, CWeaponHistory);
        newHistory.Add(NameResource.SpecialWeaponHistoryName, SWeaponHistory);
        _historyService.SaveGachaHistory(newHistory);

        GC.SuppressFinalize(this);
    }
}
