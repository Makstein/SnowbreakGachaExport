// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using System.Collections.ObjectModel;
using Vanara.Extensions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized = false;

    private ISnowbreakConfig? _configService;
    private AppConfig? _config;

    private int _selectedGamePlatformIndex;
    public int SelectedGamePlatformIndex
    {
        get => _selectedGamePlatformIndex;
        set
        {
            SetProperty(ref _selectedGamePlatformIndex, value);

            _config!.GamePlatform = (GamePlatform)value;
            _configService!.SetConfig(_config);
        }
    }

    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();

        _configService = App.GetService<ISnowbreakConfig>();
        _config = _configService?.GetConfig();
        SelectedGamePlatformIndex = (int)_config!.GamePlatform;
    }

    // Save when leave setting page 
    // TODO: Save when in setting page and exit
    public void OnNavigatedFrom()
    {
        if (_config == null)
            return;

        _configService?.SetConfig(_config);
    }

    private void InitializeViewModel()
    {
        CurrentTheme = ApplicationThemeManager.GetAppTheme();
        AppVersion = $"SnowbreakToolbox - {GetAssemblyVersion()}";

        _isInitialized = true;
    }

    private string GetAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? string.Empty;
    }

    [RelayCommand]
    private void OnChangeTheme(string parameter)
    {
        switch (parameter)
        {
            case "theme_light":
                if (CurrentTheme == ApplicationTheme.Light)
                    break;

                ApplicationThemeManager.Apply(ApplicationTheme.Light);
                CurrentTheme = ApplicationTheme.Light;
                _config!.UserPreferTheme = "Light";
                break;

            default:
                if (CurrentTheme == ApplicationTheme.Dark)
                    break;

                ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                _config!.UserPreferTheme = "Dark";
                CurrentTheme = ApplicationTheme.Dark;
                break;
        }
    }
}
