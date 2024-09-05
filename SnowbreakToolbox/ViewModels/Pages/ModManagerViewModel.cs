using Microsoft.Win32;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using SnowbreakToolbox.Tools;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Serilog;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class DisplayCharacterCategory : ObservableObject
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [ObservableProperty] private Dictionary<string, ObservableCollection<ModPakInfo>> _mods = [];
}

public partial class ModManagerViewModel : ObservableObject, INavigationAware, IDisposable
{
    private AppConfig? _config;
    private ModConfig? _modConfig;
    private bool _isInitialized;
    private StackPanel? _selectModPathPanel;

    public ObservableCollection<DisplayCharacterCategory> CharacterMods { get; set; } = [];

    [ObservableProperty] private string _dialogModPath = string.Empty;

    public ModManagerViewModel(ISnowbreakConfig snowbreakConfig, IModService modService)
    {
        _config = snowbreakConfig.GetConfig();
        _modConfig = modService.GetModConfig();
        
        Initialize();
    }

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo()
    {
        
    }
    
    private void Initialize()
    {
        if (_isInitialized) return;

        InitCharacterMods();
        _isInitialized = true;
    }

    [RelayCommand]
    private async Task ImportModPak()
    {
        try
        {
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Import Mod Pak File",
                Filter = "Pak File|*.pak",
                Multiselect = false,
            };
            if (openFileDialog.ShowDialog() != true) return;

            await AddMod(openFileDialog.FileName);
        }
        catch (Exception ex)
        {
            Log.Error($"Error in import mod pak file: {ex.Message}", ex);

            var msgBox = new MessageBox()
            {
                Title = "导入失败",
                Content = ex.Message,
                CloseButtonText = "确定",
                IsPrimaryButtonEnabled = false
            };

            await msgBox.ShowDialogAsync();
        }
    }

    private async Task AddMod(string modPath)
    {
        await Task.Run(async () =>
        {
            await EnsureModPath();

            var modName = Path.GetFileNameWithoutExtension(modPath);
            var modInfo = PakOperations.ReadPakFromPathBrute(modPath);
            if (modInfo == null)
            {
                throw new Exception($"Mod文件读取失败，可能是因为此为非角色Mod: {modName}");
            }
            if (Path.GetExtension(modPath) == ".pak")
            {
                modInfo.IsEnabled = true;
            }

            // Find the character name which the mod belongs to
            foreach (var character in CharacterMods)
            {
                if (!modInfo.CharacterCode.Contains(character.Code)) continue;

                var charCode = _modConfig!.Characters.FirstOrDefault(x => modInfo.CharacterCode.Contains(x.Code));
                var charName = charCode!.ArmorCodeNames[modInfo.CharacterCode];

                if (character.Mods[charName].Any(mod => FileOperations.AreFilesEqual(mod.ModPath, modInfo.ModPath)))
                {
                    throw new Exception($"Mod已经存在 {modInfo.Name}");
                }

                Application.Current.Dispatcher.Invoke(() => { character.Mods[charName].Add(modInfo); });
                break;
            }

            // Copy the mod to the mod folder
            var modDestPath = Path.Combine(_config!.ModPath, Path.GetFileName(modPath));
            if (string.Equals(modPath, modDestPath)) return;
            File.Copy(modPath,
                Path.Combine(_config.ModPath, Path.GetFileName(modPath)), true);
            modInfo.ModPath = modDestPath;
        });
    }

    private void InitSelectModPathPanel()
    {
        Wpf.Ui.Controls.TextBlock textBlock = new()
        {
            Text = "路径：",
            VerticalAlignment = VerticalAlignment.Center,
        };
        var textBinding = new Binding("DialogModPath")
        {
            Source = this,
        };
        TextBox textBox = new()
        {
            Margin = new Thickness(15, 0, 0, 0),
            MinWidth = 350,
            ToolTip = "（游戏文件夹下 data/game/Game/Content/Paks , Steam平台从Game文件夹开始）"
        };
        textBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, textBinding);
        Wpf.Ui.Controls.Button button = new()
        {
            Margin = new Thickness(15, 0, 0, 0),
            Command = SelectModFolderCommand,
            Content = "..."
        };
        _selectModPathPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Horizontal
        };
        _selectModPathPanel.Children.Add(textBlock);
        _selectModPathPanel.Children.Add(textBox);
        _selectModPathPanel.Children.Add(button);
        _selectModPathPanel.Visibility = Visibility.Visible;
    }

    [RelayCommand]
    private void OnSelectModFolder()
    {
        OpenFolderDialog openFolderDialog = new()
        {
            Multiselect = false,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };

        if (openFolderDialog.ShowDialog() != true)
            return;

        if (openFolderDialog.FolderNames.Length == 0)
            return;

        DialogModPath = openFolderDialog.FolderNames[0];
    }

    [RelayCommand]
    private async Task RefreshMods()
    {
        try
        {
            await EnsureModPath();

            var modFolder = _config!.ModPath;
            foreach (var modFile in Directory.GetFiles(modFolder))
            {
                try
                {
                    var extension = Path.GetExtension(modFile);
                    if (extension != ".pak" && extension != ".disabled") continue;
                    await AddMod(modFile);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error in refresh mod pak file: {ex.Message}", ex);
                }
            }
            
            var msgBox = new MessageBox()
            {
                Title = "消息",
                MinWidth = 120,
                Content = "导入完成",
                CloseButtonText = "确定",
                IsPrimaryButtonEnabled = false
            };

            await msgBox.ShowDialogAsync();
        }
        catch (Exception ex)
        {
            Log.Error($"Error in refresh mod pak file: {ex.Message}", ex);
            
            var msgBox = new MessageBox()
            {
                Title = "导入Mod失败",
                Content = ex.Message,
                CloseButtonText = "确定",
                IsPrimaryButtonEnabled = false
            };

            await msgBox.ShowDialogAsync();
        }
    }

    private async Task EnsureModPath()
    {
        if (_config == null)
        {
            throw new Exception("配置文件读取失败");
        }

        if (string.IsNullOrEmpty(_config.ModPath))
        {
            if (_selectModPathPanel == null)
            {
                Application.Current.Dispatcher.Invoke(InitSelectModPathPanel);
            }

            var result = await App.GetService<IContentDialogService>()!.ShowSimpleDialogAsync(
                new SimpleContentDialogCreateOptions()
                {
                    Title = "选择Mod路径",
                    Content = _selectModPathPanel!,
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消"
                }
            );
            if (result == ContentDialogResult.Secondary)
            {
                return;
            }
            _config.ModPath = Path.Combine(DialogModPath, "~mods");
            Directory.CreateDirectory(_config.ModPath);
        }
        else
        {
            if (!Directory.Exists(_config.ModPath)) Directory.CreateDirectory(_config.ModPath);
        }
    }

    /// <summary>
    /// Read mods from mod config
    /// </summary>
    private void InitCharacterMods()
    {
        if (_modConfig == null) return;

        var characters = _modConfig.Characters;
        foreach (var character in characters)
        {
            var displayCharacter = new DisplayCharacterCategory() { Name = character.Name, Code = character.Code };
            foreach (var (armorCode, armorName) in character.ArmorCodeNames)
            {
                var armorMods = _modConfig.Mods.Where(x => x.CharacterCode == armorCode);
                displayCharacter.Mods[armorName] = new ObservableCollection<ModPakInfo>(armorMods);
            }

            CharacterMods.Add(displayCharacter);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        if (_modConfig == null) return;

        var allMods = new List<ModPakInfo>();
        foreach (var characterMod in CharacterMods)
        {
            foreach (var (_, mods) in characterMod.Mods)
            {
                allMods.AddRange(mods);
            }
        }

        _modConfig.Mods = allMods;
    }
}