using SnowbreakGachaExport.Models;
using SnowbreakGachaExport.Tools;
using SnowbreakGachaExport.Views.Controls;
using System.Collections.Generic;

namespace SnowbreakGachaExport.ViewModels;

public class GachaLogViewModel : MainPageViewModelBase
{
    public Dictionary<string, List<HistoryItem>> HistoryItems { get; set; }

    public PoolLogControl CommonCharacterLogView { get; set; }

    public PoolLogControl SpecialCharacterLogView { get; set; }

    public PoolLogControl CommonWeaponLogView { get; set; }

    public PoolLogControl SpecialWeaponLogView { get; set; }

    private List<PoolLogControlViewModel> _viewModels = new();


    public GachaLogViewModel()
    {
        HistoryItems = JsonOperate.ReadHistory();

        InitViews();
    }

    private void InitViews()
    {
        var vm = new PoolLogControlViewModel(HistoryItems[Resource.CommonCharacterHisoryName], 80);
        CommonCharacterLogView = new PoolLogControl { DataContext = vm };
        _viewModels.Add(vm);

        vm = new PoolLogControlViewModel(HistoryItems[Resource.CommonWeaponHistoryName], 60);
        CommonWeaponLogView = new PoolLogControl { DataContext= vm };
        _viewModels.Add(vm);

        vm = new PoolLogControlViewModel(HistoryItems[Resource.SpecialCharacterHistoryName], 80);
        SpecialCharacterLogView = new PoolLogControl { DataContext = vm};
        _viewModels.Add(vm);

        vm = new PoolLogControlViewModel(HistoryItems[Resource.SpecialWeaponHistoryName], 60);
        SpecialWeaponLogView = new PoolLogControl { DataContext = vm};
        _viewModels.Add(vm);
    }

}
