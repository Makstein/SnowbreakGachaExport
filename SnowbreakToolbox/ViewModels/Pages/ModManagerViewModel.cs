using Microsoft.Win32;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using SnowbreakToolbox.Tools;
using System.Collections.ObjectModel;
using System.IO;
using Serilog;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class DisplayCharacterCategory : ObservableObject
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [ObservableProperty]
    private Dictionary<string, ObservableCollection<ModPakInfo>> _mods = [];
}

public class DisplayMod
{
    public DisplayMod(Mod mod)
    {
        Name = mod.Name;
        Description = mod.Description;
        IsEnabled = mod.IsEnabled;
    }

    public DisplayMod(string name, string description, bool isEnabled)
    {
        Name = name;
        Description = description;
        IsEnabled = isEnabled;
    }

    public string Description { get; set; }
    public bool IsEnabled { get; set; }
    public string Name { get; set; }
}

public partial class ModManagerViewModel : ObservableObject, INavigationAware, IDisposable
{
    private static readonly string DisabledSuffix = ".pak.disable";
    private static readonly string EnabledSuffix = ".pak";
    private AppConfig? _config;
    private ModConfig? _modConfig;
    private bool _isInitialized = false;
    private string _modPath = string.Empty;
    public ObservableCollection<DisplayCharacterCategory> CharacterMods { get; set; } = [];
    public ObservableCollection<DisplayMod> DisplayMods { get; set; } = [];
    public List<Mod> Mods { get; set; } = [];
    
    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo()
    {
        // Reload config every time, for hot reload
        _config = App.GetService<ISnowbreakConfig>()?.GetConfig();
        _modConfig = App.GetService<IModService>()?.GetModConfig();

        Initialize();
    }
    
    [RelayCommand]
    private void Apply()
    {
        foreach (var displayMod in DisplayMods)
        {
            var isEnable = displayMod.IsEnabled;
            var files = Directory.GetFiles(_modPath);
            foreach (var file in files)
            {
                var disableName = $"{_modPath}\\{displayMod.Name}{DisabledSuffix}";
                var enableName = $"{_modPath}\\{displayMod.Name}{EnabledSuffix}";
                if (file == disableName && displayMod.IsEnabled)
                {
                    File.Copy(file, $"{_modPath}\\{displayMod.Name}{EnabledSuffix}");
                    File.Delete(file);
                }
                else if (file == enableName && !displayMod.IsEnabled)
                {
                    File.Copy(file, $"{_modPath}\\{displayMod.Name}{DisabledSuffix}");
                    File.Delete(file);
                }
            }
        }
    }

    [RelayCommand]
    private async Task ImportModPak()
    {
        try
        {
            await Task.Run(() =>
            {
                var openFileDialog = new OpenFileDialog()
                {
                    Title = "Import Mod Pak File",
                    Filter = "Pak File|*.pak",
                    Multiselect = false,
                };
                if (openFileDialog.ShowDialog() != true) return;

                //var pak = PakOperations.ReadPakFromPathUnpack(openFileDialog.FileName);
                var modInfo = PakOperations.ReadPakFromPathBrute(openFileDialog.FileName);

                foreach (var character in CharacterMods)
                {
                    if (!modInfo.CharacterCode.Contains(character.Code)) continue;

                    var charCode = _modConfig!.Characters.FirstOrDefault(x => modInfo.CharacterCode.Contains(x.Code));
                    var charName = charCode!.ArmorCodeNames[modInfo.CharacterCode];
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        character.Mods[charName].Add(modInfo);
                    });
                    break;
                }
            });
        }
        catch(Exception ex)
        {
            Log.Error("Error in import mod pak file: {ex}", ex);
        }
    }

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
    
    private void Initialize()
    {
        if (_isInitialized) return;
        
        //UpdateDisplay();
        InitCharacterMods();
        _isInitialized = true;
    }
    
    private void ListAllMods(string modPath)
    {
        Mods.Clear();
        var files = Directory.GetFiles(_modPath);
        foreach (var file in files)
        {
            if (file.EndsWith(DisabledSuffix))
            {
                var name = file.Substring(modPath.Length + 1, file.Length - DisabledSuffix.Length - modPath.Length - 1);
                Mods.Add(new Mod(name, string.Empty, false));
            }
            else if (file.EndsWith(EnabledSuffix))
            {
                var name = file.Substring(modPath.Length + 1, file.Length - EnabledSuffix.Length - modPath.Length - 1);
                Mods.Add(new Mod(name, string.Empty, true));
            }
        }
    }
    
    [RelayCommand]
    private void SelectModFolder()
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Title = "请选择Mod文件夹",
            Multiselect = false
        };
        if (openFolderDialog.ShowDialog() != true) return;
        if (string.IsNullOrEmpty(openFolderDialog.FolderName)) return;
        _modPath = openFolderDialog.FolderName;
        ListAllMods(_modPath);
        UpdateDisplay();
    }

    [RelayCommand]
    private void UpdateDisplay()
    {
        DisplayMods.Clear();
        ListAllMods(_modPath);
        foreach (var mod in Mods)
        {
            DisplayMods.Insert(0, new DisplayMod(mod));
        }
    }

    public void Dispose()
    {
        if (_modConfig == null) return;
        
        var allMods = new List<ModPakInfo>();
        foreach (var characterMod in CharacterMods)
        {
            foreach (var (armorName, mods) in characterMod.Mods)
            {
                allMods.AddRange(mods);
            }
        }

        _modConfig.Mods = allMods;
        App.GetService<IModService>()!.Save();
        
        GC.SuppressFinalize(this);
    }
}