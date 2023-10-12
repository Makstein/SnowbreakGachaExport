using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using SnowbreakGachaExport.Models;
using SnowbreakGachaExport.Tools;
using SnowbreakGachaExport.Views.Controls;
using MsBox.Avalonia;
using System.Collections.Immutable;
using System.Diagnostics;

namespace SnowbreakGachaExport.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ICommand RefreshCommand { get; }
    public ObservableCollection<string> WindowTitleList { get; }
    public int BannerSelectedIndex { get; set; }

    private List<HistoryItem> CommonCharacterHistory { get; set; } = new();
    private List<HistoryItem> SpecialCharacterHistory { get; set; } = new();
    private List<HistoryItem> CommonWeaponHistory { get; set; } = new();
    private List<HistoryItem> SpecialWeaponHistory { get; set; } = new();
    private PoolLogControlViewModel CommonCharacterVM { get; set; }
    private PoolLogControlViewModel SpecialCharacterVM { get; set; }
    private PoolLogControlViewModel CommonWeaponVM { get; set; }
    private PoolLogControlViewModel SpecialWeaponVM { get; set; }

    private Dictionary<string, List<HistoryItem>>? _cacheDic { get; set; }

    private List<PoolLogControlViewModel> _bannerLogVmName { get; } = new();

    public PoolLogControl CommonCharacterLogView { get; set; }
    public PoolLogControl SpecialCharacterLogView { get; set; }
    public PoolLogControl CommonWeaponLogView { get; set; }
    public PoolLogControl SpecialWeaponLogView { get; set; }

    private readonly AppConfig _config;
    private readonly BitMapPool _bitMapPool;

    public MainWindowViewModel()
    {
        RefreshCommand = ReactiveCommand.CreateFromTask<int>(StartRefresh);
        
        WindowTitleList = new ObservableCollection<string>(WindowOperate.FindAll());

        InitHistory();
        InitViews();

        _config = new AppConfig();
        _bitMapPool = new BitMapPool(_config);
    }
    
    private void InitHistory()
    {
        // Read History Cache
        _cacheDic = JsonOperate.Read();
        if (_cacheDic != null)
        {
            CommonCharacterHistory = new List<HistoryItem>(_cacheDic[nameof(CommonCharacterHistory)]);
            SpecialCharacterHistory = new List<HistoryItem>(_cacheDic[nameof(SpecialCharacterHistory)]);
            CommonWeaponHistory = new List<HistoryItem>(_cacheDic[nameof(CommonWeaponHistory)]);
            SpecialWeaponHistory = new List<HistoryItem>(_cacheDic[nameof(SpecialWeaponHistory)]);
        }
        else
        {
            _cacheDic = new Dictionary<string, List<HistoryItem>>()
            {
                {nameof(CommonCharacterHistory), CommonCharacterHistory},
                {nameof(SpecialCharacterHistory), SpecialCharacterHistory},
                {nameof(CommonWeaponHistory), CommonWeaponHistory},
                {nameof(SpecialWeaponHistory), SpecialWeaponHistory}
            };
        }
    }

    private void InitViews()
    {
        CommonCharacterVM = new PoolLogControlViewModel(CommonCharacterHistory, 80);
        SpecialCharacterVM = new PoolLogControlViewModel(SpecialCharacterHistory, 80);
        CommonWeaponVM = new PoolLogControlViewModel(CommonWeaponHistory, 60);
        SpecialWeaponVM = new PoolLogControlViewModel(SpecialWeaponHistory, 60);

        // 生成池子序号和ViewModel的对应关系，以便于下面更新
        _bannerLogVmName.Add(CommonCharacterVM);
        _bannerLogVmName.Add(SpecialCharacterVM);
        _bannerLogVmName.Add(CommonWeaponVM);
        _bannerLogVmName.Add(SpecialWeaponVM);

        CommonCharacterLogView = new PoolLogControl()
        {
            DataContext = CommonCharacterVM
        };
        SpecialCharacterLogView = new PoolLogControl()
        {
            DataContext = SpecialCharacterVM
        };
        CommonWeaponLogView = new PoolLogControl()
        {
            DataContext = CommonWeaponVM
        };
        SpecialWeaponLogView = new PoolLogControl()
        {
            DataContext = SpecialWeaponVM
        };
    }

    private async Task StartRefresh(int windowIndex)
    {
        try
        {
            if (windowIndex < 0)
            {
                var errBox = MessageBoxManager.GetMessageBoxStandard("", "Please select a game window");
                await errBox.ShowWindowAsync();
                return;
            }

            var gameWindowTitle = WindowTitleList[windowIndex];

            if (!_config.IsInit)
                _config.Init(gameWindowTitle);

            WindowOperate.BringToFront(gameWindowTitle);

            var items = await PxFind.IdentifyHistories(_bitMapPool, _config);

            MergeHistory(items);

            //MergeHistory(newItems, _bannerName[BannerSelectedIndex]);

            //_bannerLogVmName[BannerSelectedIndex].UpdateList(_cacheDic[_bannerName[BannerSelectedIndex]]);
            //JsonOperate.Save(_cacheDic!);

            WindowOperate.BringToFront("SnowbreakGachaExportTool");

            //var msgBox = MessageBoxManager.GetMessageBoxStandard("", "Finished!");
            //await msgBox.ShowWindowAsync();
        }
        catch (Exception e)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandard("", "Error when start refresh: " + e.Message);
            await msgBox.ShowWindowAsync();
            throw;
        }
    }

    private void MergeHistory(ImmutableArray<HistoryItem> items)
    {
        foreach (var item in items)
        {
            Debug.WriteLine(item.ToString());
        }
    }

    private void MergeHistory(List<HistoryItem> newList, string name)
    {
        if (_cacheDic == null)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandard("", "Error: CacheDic Null");
            _ = msgBox.ShowWindowAsync();
            return;
        }

        if (_cacheDic[name].Count == 0)
        {
            _cacheDic[name].AddRange(newList);
            return;
        }
        
        var i = 0;
        while (i < newList.Count && newList[i].ID != _cacheDic[name][0].ID)
        {
            ++i;
        }

        _cacheDic[name].Reverse();
        _cacheDic[name].AddRange(newList.Take(i).Reverse());
        _cacheDic[name].Reverse();
    }
}