using Microsoft.Win32;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using SnowbreakToolbox.Tools;
using System.Collections.ObjectModel;
using System.IO;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.ViewModels.Pages;

public class DisplayCharacterCategory
{
    public string Code { get; set; } = string.Empty;
    public ObservableCollection<DisplayMod> Mods { get; set; } = [];
    public string Name { get; set; } = string.Empty;
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

    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string Name { get; set; } = string.Empty;
}

public partial class ModManagerViewModel : ObservableObject, INavigationAware
{
    private readonly static string DisabledSuffix = ".pak.disable";
    private readonly static string EnabledSuffix = ".pak";
    private AppConfig? _config;
    private bool IsInitialized = false;
    private string ModPath = string.Empty;
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

        Initialize();
    }
    [RelayCommand]
    private void Apply()
    {
        foreach (var displayMod in DisplayMods)
        {
            var isEnable = displayMod.IsEnabled;
            var files = Directory.GetFiles(ModPath);
            foreach (var file in files)
            {
                var disableName = $"{ModPath}\\{displayMod.Name}{DisabledSuffix}";
                var enableName = $"{ModPath}\\{displayMod.Name}{EnabledSuffix}";
                if (file == disableName && displayMod.IsEnabled)
                {
                    File.Copy(file, $"{ModPath}\\{displayMod.Name}{EnabledSuffix}");
                    File.Delete(file);
                }
                else if (file == enableName && !displayMod.IsEnabled)
                {
                    File.Copy(file, $"{ModPath}\\{displayMod.Name}{DisabledSuffix}");
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
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Title = "Import Mod Pak File",
                    Filter = "Pak File|*.pak",
                    Multiselect = false,
                };
                if (openFileDialog.ShowDialog() != true) return;

                //var pak = PakOperations.ReadPakFromPathUnpack(openFileDialog.FileName);
                var pak = PakOperations.ReadPakFromPathBrute(openFileDialog.FileName);
            });
        }
        catch
        {
        }
    }

    private void InitCharacterMods()
    {
        if (_config == null) return;

        var characters = _config.CharacterCodeName;
        foreach (var (k, v) in characters)
        {
            CharacterMods.Add(new DisplayCharacterCategory() { Name = v, Code = k });
        }
    }

    private void Initialize()
    {
        if (!IsInitialized)
        {
            //UpdateDisplay();
            InitCharacterMods();
            IsInitialized = true;
        }
    }
    private void ListAllMods(string modPath)
    {
        Mods.Clear();
        var files = Directory.GetFiles(ModPath);
        foreach (var file in files)
        {
            if (file.EndsWith(DisabledSuffix))
            {
                var name = file.Substring(ModPath.Length + 1, file.Length - DisabledSuffix.Length - ModPath.Length - 1);
                Mods.Add(new Mod(name, string.Empty, false));
            }
            else if (file.EndsWith(EnabledSuffix))
            {
                var name = file.Substring(ModPath.Length + 1, file.Length - EnabledSuffix.Length - ModPath.Length - 1);
                Mods.Add(new Mod(name, string.Empty, true));
            }
        }
    }
    [RelayCommand]
    private void SelectModFolder()
    {
        OpenFolderDialog openFolderDialog = new OpenFolderDialog();
        openFolderDialog.Title = "请选择Mod文件夹";
        openFolderDialog.Multiselect = false;
        if (openFolderDialog.ShowDialog() != true) return;
        if (string.IsNullOrEmpty(openFolderDialog.FolderName)) return;
        ModPath = openFolderDialog.FolderName;
        ListAllMods(ModPath);
        UpdateDisplay();
    }

    [RelayCommand]
    private void UpdateDisplay()
    {
        DisplayMods.Clear();
        ListAllMods(ModPath);
        foreach (var mod in Mods)
        {
            DisplayMods.Insert(0, new DisplayMod(mod));
        }
    }
}