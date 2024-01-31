using MsBox.Avalonia;
using ReactiveUI;
using SnowbreakGachaExport.Models;
using SnowbreakGachaExport.Tools;
using SnowbreakGachaExport.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SnowbreakGachaExport.ViewModels;

public class GachaLogViewModel : MainPageViewModelBase
{
    public Dictionary<string, List<HistoryItem>> HistoryItems { get; set; }

    public PoolLogControl CommonCharacterLogView { get; set; }

    public PoolLogControl SpecialCharacterLogView { get; set; }

    public PoolLogControl CommonWeaponLogView { get; set; }

    public PoolLogControl SpecialWeaponLogView { get; set; }

    public ICommand RefreshCommand { get; }

    public int SelectedBannerIndex { get; set; }

    private readonly List<PoolLogControlViewModel> _viewModels = new();
    private readonly AppConfig _config;
    private readonly BitMapPool _bitMapPool;
    private readonly string[] BannerNames = { Resource.CommonCharacterHisoryName, Resource.CommonWeaponHistoryName, Resource.SpecialCharacterHistoryName, Resource.SpecialWeaponHistoryName };

    public GachaLogViewModel()
    {
        HistoryItems = JsonOperate.ReadHistory();
        RefreshCommand = ReactiveCommand.CreateFromTask(StartRefresh);
        _config = new AppConfig();
        _bitMapPool = new BitMapPool(_config);

        InitLocalHistory();
        InitViews();
    }

    private void InitViews()
    {
        // 初始化四个抽卡记录显示界面
        
        var vm = new PoolLogControlViewModel(HistoryItems[Resource.CommonCharacterHisoryName], 80);
        CommonCharacterLogView = new PoolLogControl { DataContext = vm };
        _viewModels.Add(vm);

        vm = new PoolLogControlViewModel(HistoryItems[Resource.CommonWeaponHistoryName], 60);
        CommonWeaponLogView = new PoolLogControl { DataContext = vm };
        _viewModels.Add(vm);

        vm = new PoolLogControlViewModel(HistoryItems[Resource.SpecialCharacterHistoryName], 80);
        SpecialCharacterLogView = new PoolLogControl { DataContext = vm };
        _viewModels.Add(vm);

        vm = new PoolLogControlViewModel(HistoryItems[Resource.SpecialWeaponHistoryName], 60);
        SpecialWeaponLogView = new PoolLogControl { DataContext = vm };
        _viewModels.Add(vm);
    }

    private void InitLocalHistory()
    {
        HistoryItems = JsonOperate.ReadHistory();
    }

    private async Task StartRefresh()
    {
        try
        {
            var gameConfig = JsonOperate.ReadConfig();
            if (string.IsNullOrEmpty(gameConfig.GameWindowTitle))
            {
                var errBox = MessageBoxManager.GetMessageBoxStandard("", "Please select a game window in setting page");
                await errBox.ShowWindowAsync();
                return;
            }

            var gameWindowTitle = gameConfig.GameWindowTitle;

            if (!_config.IsInit)
                _config.Init(gameWindowTitle);

            WindowOperate.BringToFront(gameWindowTitle);

            Task.Delay(200).Wait();

            var items = await PxFind.IdentifyHistories(_bitMapPool, _config);

            MergeHistory(items);
            _viewModels[SelectedBannerIndex].UpdateList(HistoryItems[BannerNames[SelectedBannerIndex]]);
            JsonOperate.SaveHistory(HistoryItems);

            WindowOperate.BringToFront("SnowbreakGachaExportTool");
            var msgBox = MessageBoxManager.GetMessageBoxStandard("", "Finished!");
            await msgBox.ShowWindowAsync();
        }
        catch (Exception e)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandard("", "Error when start refresh: " + e.Message);
            await msgBox.ShowWindowAsync();
            throw;
        }
    }

    private void MergeHistory(ImmutableArray<HistoryItem> newItems)
    {
        // 若本地历史记录为空则直接添加本次统计的历史记录
        if (HistoryItems[BannerNames[SelectedBannerIndex]].Count == 0)
        {
            HistoryItems[BannerNames[SelectedBannerIndex]].AddRange(newItems);
            return;
        }

        // 寻找本次统计的历史记录与本地历史记录中的最早重复节点
        var i = 0;
        while (i < newItems.Length && newItems[i].ID != _viewModels[SelectedBannerIndex].LogList[0].ID)
        {
            ++i;
        }
        HistoryItems[BannerNames[SelectedBannerIndex]].InsertRange(0, newItems.Take(i));
    }
}
