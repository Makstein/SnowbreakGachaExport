using Avalonia.Controls;
using Newtonsoft.Json;
using SnowbreakGachaExport.Models.Global;
using SnowbreakGachaExport.Tools;
using System.Collections.ObjectModel;
using System.IO;

namespace SnowbreakGachaExport.ViewModels;

public class SettingViewModel : MainPageViewModelBase
{
    public ObservableCollection<string> WindowTitleList { get; set; }

    private string _selectedWindowTitle;
    public string SelectedWindowTitle
    {
        get => _selectedWindowTitle;
        set
        {
            if (value != _selectedWindowTitle)
            {
                _selectedWindowTitle = value;

                if (Design.IsDesignMode) return;

                _gameConfig.GameWindowTitle = value;
                JsonOperate.SaveConfig(ref _gameConfig);
            }
        }
    }

    private GameConfig _gameConfig;

    public SettingViewModel()
    {
        WindowTitleList = new ObservableCollection<string>(WindowOperate.FindAll());

        if (Design.IsDesignMode) return;

        _gameConfig = JsonOperate.ReadConfig();

        _selectedWindowTitle = _gameConfig.GameWindowTitle;
    }
}
