using Microsoft.Win32;
using Serilog;
using SnowbreakToolbox.Global;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using SnowbreakToolbox.Services;
using SnowbreakToolbox.Tools;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vanara.PInvoke;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.ViewModels.Pages;

public class DisplayItem(string armorName, string characterName, int count)
{
    public string ArmorName { get; } = armorName;
    public string CharacterName { get; } = characterName;
    public int Count { get; } = count; // Count since last 5-star character
}

public partial class GachaHistoryViewModel(
    ISnowbreakOcr snowbreakOcr,
    ISnowbreakConfig snowbreakConfig,
    ISnowbreakHistory snowbreakHistory) : ObservableObject, INavigationAware, IDisposable
{
    private readonly PaddleOrcService _paddleOrcService = (snowbreakOcr as PaddleOrcService)!;
    private readonly ISnowbreakConfig _configService = snowbreakConfig;

    private readonly ISnowbreakHistory _historyService = snowbreakHistory;

    // The height of title bar in Windows is 30px
    private const int WindowsTitleBarHeight = 30;

    private AppConfig? _config;
    private bool _initialized;

    [ObservableProperty] private double _avgCc; // Common character
    [ObservableProperty] private int _minCc;
    [ObservableProperty] private int _primeCountCc;

    [ObservableProperty] private double _avgCw; // Common weapon
    [ObservableProperty] private int _minCw;
    [ObservableProperty] private int _primeCountCw;

    [ObservableProperty] private double _avgSc; // Special character
    [ObservableProperty] private int _minSc;
    [ObservableProperty] private int _primeCountSc;

    [ObservableProperty] private double _avgSw; // Special weapon
    [ObservableProperty] private int _minSw;
    [ObservableProperty] private int _primeCountSw;

    [ObservableProperty] private double _avgScm; // Special character Mihoyo
    [ObservableProperty] private int _minScm;
    [ObservableProperty] private int _primeCountScm;

    [ObservableProperty] private double _avgSwm; // Special weapon Mihoyo
    [ObservableProperty] private int _minSwm;
    [ObservableProperty] private int _primeCountSwm;

    public int SelectedBannerIndex { get; set; }

    private List<GachaItem> CCharHistory { get; set; } = []; // Common Character
    private List<GachaItem> SCharHistory { get; set; } = []; // Special Character
    private List<GachaItem> CWeaponHistory { get; set; } = [];
    private List<GachaItem> SWeaponHistory { get; set; } = [];
    private List<GachaItem> SCharHistoryMihoyo { get; set; } = [];
    private List<GachaItem> SWeaponHistoryMihoyo { get; set; } = [];

    public ObservableCollection<DisplayItem> DisplayCCharHistory { get; } =
        []; // Collection for display in page(to show count)

    // Use for detect armor name
    private const string DashPattern = @"[\u2012\u2013\u2014\u2015]";
    private static Regex _dashRegex = new(DashPattern);

    public ObservableCollection<DisplayItem> DisplaySCharHistory { get; } = [];
    public ObservableCollection<DisplayItem> DisplayCWeaponHistory { get; } = [];
    public ObservableCollection<DisplayItem> DisplaySWeaponHistory { get; } = [];
    public ObservableCollection<DisplayItem> DisplaySCharHistoryMihoyo { get; } = [];
    public ObservableCollection<DisplayItem> DisplaySWeaponHistoryMihoyo { get; } = [];

    [RelayCommand]
    private void OnGetHistory()
    {
        try
        {
            var gameWindowHwnd = User32.FindWindow(null, _config!.GameWindowTitle);
            if (gameWindowHwnd == HWND.NULL)
                gameWindowHwnd = User32.FindWindow(null, _config.GameWindowTitleCN);
            if (gameWindowHwnd == HWND.NULL)
            {
                throw new Exception("Exception: can't find game window");
            }

            User32.GetWindowRect(gameWindowHwnd, out var rect);
            var bFullscreen = IsFullscreen(rect.Width, rect.Height);

            var gameWindowWidth = rect.Width;
            var gameWindowHeight = bFullscreen ? rect.Height : rect.Height - WindowsTitleBarHeight;

            User32.BringWindowToTop(gameWindowHwnd);

            _config.ClientGameScale = (double)gameWindowWidth / _config.ReferenceScreenWidth;
            if (Math.Abs(_config.ReferenceScreenHeight * _config.ClientGameScale - gameWindowHeight) > 0.01)
            {
                throw new Exception(
                    $"游戏分辨率初始化失败：非16: 9分辨率，检测游戏窗口宽高 {gameWindowWidth} : {gameWindowHeight}, 全屏：{bFullscreen}");
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

            if (!bFullscreen)
            {
                _config.ClientLogBoxY0 += WindowsTitleBarHeight;
                _config.ClientNextPageArrowY += WindowsTitleBarHeight;

                _config.ClientLogBoxY0 += rect.top;
                _config.ClientLogBoxX0 += rect.left;
                _config.ClientNextPageArrowY += rect.top;
                _config.ClientNextPageArrowX += rect.left;
                _config.ClientRareColorPosXWindowed =
                    (int)(_config.ReferenceRareColorPosXWindowed * _config.ClientGameScale);
            }

            App.GetService<ISnowbreakConfig>()?.SetConfig(_config);

            Task.Delay(200).Wait();

            var list = new List<GachaItem>();
            Bitmap? lastCapturedImage = null;
            while (true)
            {
                var image =
                    ScreenOperations.CaptureRegion(_config.ClientLogBoxX0, _config.ClientLogBoxY0,
                        _config.ClientLogBoxWidth, _config.ClientLogBoxHeight);

                if (lastCapturedImage != null)
                {
                    var mse = ImageOperations.ImageMse(lastCapturedImage, image);
                    if (mse < 45)
                    {
                        break;
                    }
                }

                lastCapturedImage = new Bitmap(image);

                //_paddleOrcService.GetText(image);
                var regions = _paddleOrcService.GetRegions(image);

                foreach (var region in regions)
                {
                    var color = image.GetPixel(
                        bFullscreen ? _config.ClientRareColorPosX : _config.ClientRareColorPosXWindowed,
                        (int)region[0].Rect.Center.Y);
                    var rare = GetRare(color);
                    var item = new GachaItem(region[0].Text, region[2].Text,
                        region[1].Text == "武器" ? ItemType.Weapon : ItemType.Character, rare);
                    list.Add(item);
                }

                MouseOperations.LeftMouseClick(_config.ClientNextPageArrowX, _config.ClientNextPageArrowY);

                Task.Delay(200).Wait();
            }

            MergeHistory(list);

            User32.ShowWindow(gameWindowHwnd, ShowWindowCommand.SW_MINIMIZE);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message);
            Log.Warning(ex.Message, ex);
        }
    }

    private bool IsFullscreen(int gameWindowWidth, int gameWindowHeight)
    {
        return gameWindowWidth >= _config!.ClientScreenWidth && gameWindowHeight >= _config.ClientScreenHeight;
    }

    private void ComputePrimeCount(int bannerIndex)
    {
        List<GachaItem> history = [];
        switch (bannerIndex)
        {
            case 0:
                history = SCharHistory;
                break;
            case 1:
                history = SWeaponHistory;
                break;
            case 2:
                history = SCharHistoryMihoyo;
                break;
            case 3:
                history = SWeaponHistoryMihoyo;
                break;
            case 4:
                history = CCharHistory;
                break;
            case 5:
                history = CWeaponHistory;
                break;
        }

        var primeCount = 0;
        foreach (var item in history.TakeWhile(x => x.Star != 5))
        {
#if DEBUG
            Log.Information($"Banner Index {bannerIndex}, item is {item.Id}");
#endif
            primeCount++;
        }

        switch (bannerIndex)
        {
            case 0:
                PrimeCountSc = primeCount;
                break;
            case 1:
                PrimeCountSw = primeCount;
                break;
            case 2:
                PrimeCountScm = primeCount;
                break;
            case 3:
                PrimeCountSwm = primeCount;
                break;
            case 4:
                PrimeCountCc = primeCount;
                break;
            case 5:
                PrimeCountCw = primeCount;
                break;
        }
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

    private static double ColorMse(Color a, Color b) =>
        Vector3.Distance(new Vector3(a.R, a.G, a.B), new Vector3(b.R, b.G, b.B));

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
                curHistory = SCharHistoryMihoyo;
                break;
            case 3:
                curHistory = SWeaponHistoryMihoyo;
                break;
            case 4:
                curHistory = CCharHistory;
                break;
            case 5:
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
                curHistory = SCharHistory;
                curDisplayHistory = DisplaySCharHistory;
                break;
            case 1:
                curHistory = SWeaponHistory;
                curDisplayHistory = DisplaySWeaponHistory;
                break;
            case 2:
                curHistory = SCharHistoryMihoyo;
                curDisplayHistory = DisplaySCharHistoryMihoyo;
                break;
            case 3:
                curHistory = SWeaponHistoryMihoyo;
                curDisplayHistory = DisplaySWeaponHistoryMihoyo;
                break;
            case 4:
                curHistory = CCharHistory;
                curDisplayHistory = DisplayCCharHistory;
                break;
            case 5:
                curHistory = CWeaponHistory;
                curDisplayHistory = DisplayCWeaponHistory;
                break;
        }

        curDisplayHistory.Clear();
        for (var i = curHistory.Count - 1; i >= 0; i--)
        {
            count++;
            if (curHistory[i].Star != 5) continue;
            if (curHistory[i].Type == ItemType.Character)
            {
                curHistory[i].Name = _dashRegex.Replace(curHistory[i].Name, "-");
                var names = curHistory[i].Name.Contains("——")
                    ? curHistory[i].Name.Split("——")
                    : curHistory[i].Name.Split('-');
                curDisplayHistory.Insert(0, new DisplayItem(names[0], names[1], count));
            }
            else
            {
                string[] names = [curHistory[i].Name, "武器"];
                curDisplayHistory.Insert(0, new DisplayItem(names[0], names[1], count));
            }

            count = 0;
        }

        if (curDisplayHistory.Count <= 0)
        {
            ComputePrimeCount(bannerIndex);
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
                AvgScm = curDisplayHistory.Average(x => x.Count);
                MinScm = curDisplayHistory.MinBy(x => x.Count)!.Count;
                break;
            case 3:
                AvgSwm = curDisplayHistory.Average(x => x.Count);
                MinSwm = curDisplayHistory.MinBy(x => x.Count)!.Count;
                break;
            case 4:
                AvgCc = curDisplayHistory.Average(x => x.Count);
                MinCc = curDisplayHistory.MinBy(x => x.Count)!.Count;
                break;
            case 5:
                AvgCw = curDisplayHistory.Average(x => x.Count);
                MinCw = curDisplayHistory.MinBy(x => x.Count)!.Count;
                break;
        }

        ComputePrimeCount(bannerIndex);
    }

    private void UpdateDisplayAll()
    {
        UpdateDisplay(0);
        UpdateDisplay(1);
        UpdateDisplay(2);
        UpdateDisplay(3);
        UpdateDisplay(4);
        UpdateDisplay(5);
    }

    private void InitializeViewModel()
    {
        if (_initialized) return;

        // Load local history cache
        var historyItems = _historyService.GetGachaHistory();
        CCharHistory = historyItems.TryGetValue(NameResource.CommonCharacterHistoryName, out var localCcHistory)
            ? localCcHistory
            : [];
        SCharHistory = historyItems.TryGetValue(NameResource.SpecialCharacterHistoryName, out var localScHistory)
            ? localScHistory
            : [];
        CWeaponHistory = historyItems.TryGetValue(NameResource.CommonWeaponHistoryName, out var localCwHistory)
            ? localCwHistory
            : [];
        SWeaponHistory = historyItems.TryGetValue(NameResource.SpecialWeaponHistoryName, out var localSwHistory)
            ? localSwHistory
            : [];
        SCharHistoryMihoyo =
            historyItems.TryGetValue(NameResource.SpecialCharacterHistoryNameMihoyo, out var localScmHistory)
                ? localScmHistory
                : [];
        SWeaponHistoryMihoyo =
            historyItems.TryGetValue(NameResource.SpecialWeaponHistoryNameMihoyo, out var localSwmHistory)
                ? localSwmHistory
                : [];

        UpdateDisplayAll();

        _initialized = true;
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

    [RelayCommand]
    private void ImportData()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "选择要导入的记录",
            Multiselect = false,
            Filter = "json文件|*.json",
            InitialDirectory = UserPaths.BasePath
        };
        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }

        var filename = openFileDialog.FileName;
        try
        {
            var historyString = File.ReadAllText(filename);
            var gachaHistory = JsonSerializer.Deserialize<Dictionary<string, List<GachaItem>>>(historyString);

            if (gachaHistory == null)
            {
                throw new Exception("Wrong json");
            }

            CCharHistory =
                gachaHistory.TryGetValue(NameResource.CommonCharacterHistoryName, out var localCcHistory)
                    ? localCcHistory
                    : [];
            SCharHistory =
                gachaHistory.TryGetValue(NameResource.SpecialCharacterHistoryName, out var localScHistory)
                    ? localScHistory
                    : [];
            CWeaponHistory =
                gachaHistory.TryGetValue(NameResource.CommonWeaponHistoryName, out var localCwHistory)
                    ? localCwHistory
                    : [];
            SWeaponHistory =
                gachaHistory.TryGetValue(NameResource.SpecialWeaponHistoryName, out var localSwHistory)
                    ? localSwHistory
                    : [];
            SCharHistoryMihoyo =
                gachaHistory.TryGetValue(NameResource.SpecialCharacterHistoryNameMihoyo,
                    out var localScmHistory)
                    ? localScmHistory
                    : [];
            SWeaponHistoryMihoyo =
                gachaHistory.TryGetValue(NameResource.SpecialWeaponHistoryNameMihoyo,
                    out var localSwmHistory)
                    ? localSwmHistory
                    : [];
            _historyService.SaveGachaHistory(gachaHistory);
            UpdateDisplayAll();
        }
        catch (Exception)
        {
            System.Windows.MessageBox.Show("Some error occured!");
        }
    }

    [RelayCommand]
    private void ExportData()
    {
        var curTimestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        var saveFileDialog = new SaveFileDialog
        {
            Title = "选择要导出的位置",
            InitialDirectory = UserPaths.BasePath,
            OverwritePrompt = true,
            AddExtension = true,
            DefaultExt = "json",
            Filter = "json文件|*.json",
            FileName = $"{curTimestamp}_export.json"
        };
        if (saveFileDialog.ShowDialog() != true)
        {
            return;
        }

        var filename = saveFileDialog.FileName;

        Dictionary<string, List<GachaItem>> newHistory = [];

        newHistory.Add(NameResource.SpecialCharacterHistoryName, SCharHistory);
        newHistory.Add(NameResource.SpecialWeaponHistoryName, SWeaponHistory);
        newHistory.Add(NameResource.SpecialCharacterHistoryNameMihoyo, SCharHistoryMihoyo);
        newHistory.Add(NameResource.SpecialWeaponHistoryNameMihoyo, SWeaponHistoryMihoyo);
        newHistory.Add(NameResource.CommonCharacterHistoryName, CCharHistory);
        newHistory.Add(NameResource.CommonWeaponHistoryName, CWeaponHistory);

        var newHistoryString = JsonSerializer.Serialize(newHistory, HistoryService._jsonOptions);
        File.WriteAllText(filename, newHistoryString);
    }

    [RelayCommand]
    private void ClearUpData()
    {
        var result = System.Windows.MessageBox.Show("你确定要执行这个操作吗？\n该操作会清除所有记录，但会在Data目录保留备份", "确认",
            System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == System.Windows.MessageBoxResult.No)
        {
            return;
        }

        SCharHistory = SWeaponHistory = SCharHistoryMihoyo = SWeaponHistoryMihoyo = CCharHistory = CWeaponHistory = [];
        _historyService.SaveGachaHistory([]);
        UpdateDisplayAll();
    }

    private void SaveHistory()
    {
        Dictionary<string, List<GachaItem>> newHistory = [];

        newHistory.Add(NameResource.SpecialCharacterHistoryName, SCharHistory);
        newHistory.Add(NameResource.SpecialWeaponHistoryName, SWeaponHistory);
        newHistory.Add(NameResource.SpecialCharacterHistoryNameMihoyo, SCharHistoryMihoyo);
        newHistory.Add(NameResource.SpecialWeaponHistoryNameMihoyo, SWeaponHistoryMihoyo);
        newHistory.Add(NameResource.CommonCharacterHistoryName, CCharHistory);
        newHistory.Add(NameResource.CommonWeaponHistoryName, CWeaponHistory);

        _historyService.SaveGachaHistory(newHistory);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (!_initialized)
        {
            return;
        }

        SaveHistory();
    }
}