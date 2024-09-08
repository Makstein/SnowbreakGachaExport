using Microsoft.Win32;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using SnowbreakToolbox.Tools;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Serilog;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using Exception = System.Exception;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace SnowbreakToolbox.ViewModels.Pages;

/// <summary>
/// The class used in the "Mod Manager" page to display mods per character
/// </summary>
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
    private IContentDialogService? _contentDialogService;
    private bool _isInitialized;
    private StackPanel? _selectModPathPanel;

    public ObservableCollection<DisplayCharacterCategory> CharacterMods { get; } = [];

    [ObservableProperty] private string _dialogModPath = string.Empty;

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo()
    {
        _contentDialogService ??= App.GetService<IContentDialogService>();
        
        // Show progress ring
        var grid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Focusable = false
        };
        grid.SetResourceReference(Panel.BackgroundProperty, "ContentDialogSmokeFill");
        var progressRing = new ProgressRing
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            IsIndeterminate = true,
            Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#1677ff")!
        };
        grid.Children.Add(progressRing);
        var contentPresenter = _contentDialogService!.GetDialogHost();
        contentPresenter!.Content = grid;
        
        // Load mod config
        var t = App.GetService<IModService>()!.GetModConfigAsync();
        t.ContinueWith(task =>
        {
            _modConfig = task.Result;
            _config = App.GetService<ISnowbreakConfig>()!.GetConfig();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Initialize();
                
                // Hide progressRing
                contentPresenter.Content = null;
            });
        });
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
            await ShowMessage("消息", "导入成功");
        }
        catch (Exception ex)
        {
            Log.Error($"Error in import mod pak file: {ex.Message}", ex);
            await ShowMessage("错误", $"导入失败 {ex.Message}");
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
            if (_modConfig == null) throw new Exception("Mod配置文件读取失败");
            
            await EnsureModPath();

            // Remove deleted mods from the mod list
            for (var i = _modConfig.Mods.Count - 1; i >= 0; i--)
            {
                if (!File.Exists(_modConfig.Mods[i].ModPath)) _modConfig.Mods.RemoveAt(i);
            }
            InitCharacterMods();
            
            // Add new mods to the mod list
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

            await ShowMessage("消息", "刷新完成");
        }
        catch (Exception ex)
        {
            Log.Error($"Error in refresh mod pak file: {ex.Message}", ex);
            await ShowMessage("错误", $"刷新失败 {ex.Message}");
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

        CharacterMods.Clear();
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

    private static async Task ShowMessage(string title, string content)
    {
        var msgBox = new MessageBox()
        {
            Title = title,
            MinWidth = 120,
            Content = content,
            CloseButtonText = "确定",
            IsPrimaryButtonEnabled = false
        };

        await msgBox.ShowDialogAsync();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (!_isInitialized) return;
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