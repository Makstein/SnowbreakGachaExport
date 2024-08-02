using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using OpenCvSharp.Internal.Vectors;
using SnowbreakToolbox.Models;
using SnowbreakToolbox.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.ViewModels.Pages;
public class DisplayMod
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
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
}
public partial class ModManagerViewModel : ObservableObject, INavigationAware
{
    public List<Mod> Mods = new List<Mod>();
    public ObservableCollection<DisplayMod> DisplayMods { get; set; } = [];

    static string EnabledSuffix = ".pak";
    static string DisabledSuffix = ".pak.disable";
    private bool IsInitialized = false;
    string ModPath = string.Empty;

    [RelayCommand]
    public void SelectModFolder()
    {
        OpenFolderDialog openFolderDialog = new OpenFolderDialog();
        openFolderDialog.Title = "请选择Mod文件夹";
        openFolderDialog.Multiselect = false;
        if (openFolderDialog.ShowDialog()!= true) return;
        if (string.IsNullOrEmpty(openFolderDialog.FolderName)) return;
        ModPath = openFolderDialog.FolderName;
        ListAllMods(ModPath);
        UpdateDisplay();
    }

    public void ListAllMods(string modPath)
    {
        Mods.Clear();
        var files = Directory.GetFiles(ModPath);
        foreach(var file in files)
        {
            if (file.EndsWith(DisabledSuffix))
            {
                var name = file.Substring(ModPath.Length+1, file.Length - DisabledSuffix.Length - ModPath.Length -1);
                Mods.Add(new Mod(name, string.Empty, false));
            }
            else if (file.EndsWith(EnabledSuffix))
            {
                var name = file.Substring(ModPath.Length+1, file.Length - EnabledSuffix.Length - ModPath.Length -1);
                Mods.Add(new Mod(name, string.Empty, true));
            }
        }
    }

    public void Initialize()
    {
        if (!IsInitialized)
        {
            ListAllMods(ModPath);
            UpdateDisplay();
            IsInitialized = true;
        }
    }

    [RelayCommand]
    public void UpdateDisplay()
    {
        DisplayMods.Clear();
        ListAllMods(ModPath);
        foreach (var mod in Mods)
        {
            DisplayMods.Insert(0, new DisplayMod(mod));
        }
    }
    [RelayCommand]
    public void Apply()
    {
        foreach (var displayMod in DisplayMods)
        {
            var isEnable = displayMod.IsEnabled;
            var files = Directory.GetFiles(ModPath);
            foreach(var file in files)
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

    public void OnNavigatedTo()
    {
        Initialize();
    }

    public void OnNavigatedFrom()
    {
    }
}