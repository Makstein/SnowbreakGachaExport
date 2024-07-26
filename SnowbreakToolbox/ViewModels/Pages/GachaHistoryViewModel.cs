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
    [ObservableProperty] private int _primeCountCc;

    [ObservableProperty] private double _avgCw;     // Common weapon
    [ObservableProperty] private int _minCw;
    [ObservableProperty] private int _primeCountCw;

    [ObservableProperty] private double _avgSc;     // Special character
    [ObservableProperty] private int _minSc;
    [ObservableProperty] private int _primeCountSc;

    [ObservableProperty] private double _avgSw;     // Special weapon
    [ObservableProperty] private int _minSw;
    [ObservableProperty] private int _primeCountSw;

    [ObservableProperty] private double _avgScm;    // Special character Mihoyo
    [ObservableProperty] private int _minScm;
    [ObservableProperty] private int _primeCountScm;

    [ObservableProperty] private double _avgSwm;    // Special weapon Mihoyo
    [ObservableProperty] private int _minSwm;
    [ObservableProperty] private int _primeCountSwm;

    public int SelectedBannerIndex { get; set; }

    public List<GachaItem> CCharHistory { get; private set; } = [];                     // Common Character
    public List<GachaItem> SCharHistory { get; private set; } = [];                     // Special Character
    public List<GachaItem> CWeaponHistory { get; private set; } = [];
    public List<GachaItem> SWeaponHistory { get; private set; } = [];
    public List<GachaItem> SCharHistoryMihoyo { get; private set; } = [];
    public List<GachaItem> SWeaponHistoryMihoyo { get; private set; } = [];

    public ObservableCollection<DisplayItem> DisplayCCharHistory { get; private set; } = [];            // Collection for display in page(to show count)
    public ObservableCollection<DisplayItem> DisplaySCharHistory { get; private set; } = [];
    public ObservableCollection<DisplayItem> DisplayCWeaponHistory { get; private set; } = [];
    public ObservableCollection<DisplayItem> DisplaySWeaponHistory { get; private set; } = [];
    public ObservableCollection<DisplayItem> DisplaySCharHistoryMihoyo { get; private set; } = [];
    public ObservableCollection<DisplayItem> DisplaySWeaponHistoryMihoyo { get; private set; } = [];

    [RelayCommand]
    private void OnGetHistory()
    {
        try {
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
        } catch (Exception)
        {
            System.Windows.MessageBox.Show("游戏分辨率初始化失败：非16: 9分辨率全屏");
            Log.Warning("游戏分辨率初始化失败：非16: 9分辨率全屏");
        }
    }

    public void ComputePrimeCount(int bannerIndex)
    {
        List<GachaItem> _history = [];
        switch (bannerIndex)
        {
            case 0:
                _history = SCharHistory;
                break;
            case 1:
                _history = SWeaponHistory;
                break;
            case 2:
                _history = SCharHistoryMihoyo;
                break;
            case 3:
                _history = SWeaponHistoryMihoyo;
                break;
            case 4:
                _history = CCharHistory;
                break;
            case 5:
                _history = CWeaponHistory;
                break;
        }
        int primeCount = 0;
        foreach (GachaItem _item in _history)
        {
            if (_item.Star == 5) { break; }
#if DEBUG
            Log.Information($"Banner Index {bannerIndex}, item is {_item.Id}");
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

        switch(bannerIndex)
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
        if (!_initialized)
        {
            // Load local history cache
            var historyItems = _historyService.GetGachaHistory();
            CCharHistory = historyItems.TryGetValue(NameResource.CommonCharacterHistoryName, out List<GachaItem>? localCcHistory) ? localCcHistory : [];
            SCharHistory = historyItems.TryGetValue(NameResource.SpecialCharacterHistoryName, out List<GachaItem>? localScHistory) ? localScHistory : [];
            CWeaponHistory = historyItems.TryGetValue(NameResource.CommonWeaponHistoryName, out List<GachaItem>? localCwHistory) ? localCwHistory : [];
            SWeaponHistory = historyItems.TryGetValue(NameResource.SpecialWeaponHistoryName, out List<GachaItem>? localSwHistory) ? localSwHistory : [];
            SCharHistoryMihoyo = historyItems.TryGetValue(NameResource.SpecialCharacterHistoryNameMihoyo, out List<GachaItem>? localScmHistory) ? localScmHistory : [];
            SWeaponHistoryMihoyo = historyItems.TryGetValue(NameResource.SpecialWeaponHistoryNameMihoyo, out List<GachaItem>? localSwmHistory) ? localSwmHistory : [];

            UpdateDisplayAll();

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

    [RelayCommand]
    public void ImportData()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Title = "选择要导入的记录";
        openFileDialog.Multiselect = false;
        openFileDialog.Filter = "json文件|*.json";
        openFileDialog.InitialDirectory = UserPaths.BasePath;
        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }
        var filename = openFileDialog.FileName;
        try {
            var HistoryString = File.ReadAllText(filename);
            var _gachaHistory = JsonSerializer.Deserialize<Dictionary<string, List<GachaItem>>>(HistoryString);

            if(_gachaHistory == null)
            {
                throw new Exception("Wrong json");
            }
            CCharHistory = _gachaHistory.TryGetValue(NameResource.CommonCharacterHistoryName, out List<GachaItem>? localCcHistory) ? localCcHistory : [];
            SCharHistory = _gachaHistory.TryGetValue(NameResource.SpecialCharacterHistoryName, out List<GachaItem>? localScHistory) ? localScHistory : [];
            CWeaponHistory = _gachaHistory.TryGetValue(NameResource.CommonWeaponHistoryName, out List<GachaItem>? localCwHistory) ? localCwHistory : [];
            SWeaponHistory = _gachaHistory.TryGetValue(NameResource.SpecialWeaponHistoryName, out List<GachaItem>? localSwHistory) ? localSwHistory : [];
            SCharHistoryMihoyo = _gachaHistory.TryGetValue(NameResource.SpecialCharacterHistoryNameMihoyo, out List<GachaItem>? localScmHistory) ? localScmHistory : [];
            SWeaponHistoryMihoyo = _gachaHistory.TryGetValue(NameResource.SpecialWeaponHistoryNameMihoyo, out List<GachaItem>? localSwmHistory) ? localSwmHistory : [];

            UpdateDisplayAll();
        }
        catch (Exception) {
            System.Windows.MessageBox.Show("Some error occured!");
        }
    }

    [RelayCommand]
    public void ExportData()
    {
        var curTimestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Title = "选择要导出的位置";
        saveFileDialog.InitialDirectory = UserPaths.BasePath;
        saveFileDialog.OverwritePrompt = true;
        saveFileDialog.AddExtension = true;
        saveFileDialog.DefaultExt = "json";
        saveFileDialog.Filter = "json文件|*.json";
        saveFileDialog.FileName = $"{curTimestamp}_export.json";
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
        return;
    }

    [RelayCommand]
    public void ClearUpData()
    {
        var result = System.Windows.MessageBox.Show("你确定要执行这个操作吗？\n该操作会清除所有记录，但会在Data目录保留备份", "确认", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
        if(result == System.Windows.MessageBoxResult.No) {  return; }
        SCharHistory = SWeaponHistory = SCharHistoryMihoyo = SWeaponHistoryMihoyo = CCharHistory = CWeaponHistory = [];
        _historyService.SaveGachaHistory([]);
        UpdateDisplayAll();
    }

    public void Dispose()
    {
        if (!_initialized)
        {
            return;
        }

        Dictionary<string, List<GachaItem>> newHistory = [];

        newHistory.Add(NameResource.SpecialCharacterHistoryName, SCharHistory);
        newHistory.Add(NameResource.SpecialWeaponHistoryName, SWeaponHistory);
        newHistory.Add(NameResource.SpecialCharacterHistoryNameMihoyo, SCharHistoryMihoyo);
        newHistory.Add(NameResource.SpecialWeaponHistoryNameMihoyo, SWeaponHistoryMihoyo);
        newHistory.Add(NameResource.CommonCharacterHistoryName, CCharHistory);
        newHistory.Add(NameResource.CommonWeaponHistoryName, CWeaponHistory);

        _historyService.SaveGachaHistory(newHistory);

        GC.SuppressFinalize(this);
    }
}
