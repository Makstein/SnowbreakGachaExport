using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using ReactiveUI;
using SnowbreakGachaExport.Models;
using SnowbreakGachaExport.Tools;
using SnowbreakGachaExport.Views.Controls;
using OpenCvSharp;

namespace SnowbreakGachaExport.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ICommand RefreshCommand { get; }
    public ICommand TestDataAddCommand { get; }
    public ObservableCollection<string> WindowTitleList { get; }

    private ObservableCollection<HistoryItem> CommonCharacterHistory { get; } = new();
    private ObservableCollection<HistoryItem> SpecialCharacterHistory { get; } = new();
    private ObservableCollection<HistoryItem> CommonWeaponHistory { get; } = new();
    private ObservableCollection<HistoryItem> SpecialWeaponHistory { get; } = new();
    private PoolLogControlViewModel CommonCharacterVM { get; }
    private PoolLogControlViewModel SpecialCharacterVM { get; }
    private PoolLogControlViewModel CommonWeaponVM { get; }
    private PoolLogControlViewModel SpecialWeaponVM { get; }

    public PoolLogControl CommonCharacterLogView { get; }
    public PoolLogControl SpecialCharacterLogView { get; }
    public PoolLogControl CommonWeaponLogView { get; }
    public PoolLogControl SpecialWeaponLogView { get; }

    public MainWindowViewModel()
    {
        RefreshCommand = ReactiveCommand.CreateFromTask<int>(StartRefresh);
        TestDataAddCommand = ReactiveCommand.Create(AddTestData);
        WindowTitleList = new ObservableCollection<string>(WindowOperate.FindAll());

        #region TestData

        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Space Cowboy", star: 5));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Space Cowboy", star: 5));
        SpecialWeaponHistory.Add(new HistoryItem("3* Weapon", star: 3));
        SpecialWeaponHistory.Add(new HistoryItem("3* Weapon", star: 3));
        SpecialWeaponHistory.Add(new HistoryItem("3* Weapon", star: 3));
        SpecialWeaponHistory.Add(new HistoryItem("3* Weapon", star: 3));
        SpecialWeaponHistory.Add(new HistoryItem("3* Weapon", star: 3));

        #endregion

        CommonCharacterVM = new PoolLogControlViewModel(CommonCharacterHistory, 80);
        SpecialCharacterVM = new PoolLogControlViewModel(SpecialCharacterHistory, 80);
        CommonWeaponVM = new PoolLogControlViewModel(CommonWeaponHistory, 70);
        SpecialWeaponVM = new PoolLogControlViewModel(SpecialWeaponHistory, 70);

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
            if (windowIndex < 0) return;
            WindowOperate.BringToFront(WindowTitleList[windowIndex]);
            //.BringToFront("tgame");
            await Task.Delay(100);
            
            while (true)
            {
                var pos = OpenCVFind.FindNextPageArrow();
                if (pos is { X: 0, Y: 0 }) break;
                
                SpecialCharacterHistory.AddRange(OpenCVFind.FindStar(ItemType.Character));
            
                MouseOperate.DoMouseClick(pos.X, pos.Y);
                await Task.Delay(100);
                MouseOperate.DoMouseClick(pos.X + 80, pos.Y + 80);
                await Task.Delay(100);
            }
            
            SpecialCharacterVM.UpdateList(SpecialCharacterHistory.Reverse());
        }
        catch (Exception e)
        {
            Console.WriteLine("Error in StartRefresh" + e);
            throw;
        }
    }

    private void AddTestData()
    {
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Big Treasure"));
        SpecialWeaponHistory.Add(new HistoryItem("Space Cowboy", star: 5));
        SpecialWeaponVM.UpdateList(SpecialWeaponHistory);
    }
}