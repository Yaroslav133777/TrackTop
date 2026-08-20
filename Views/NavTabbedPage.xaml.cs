using System;
using MauiControls = Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TrackTop;

public class NavTabbedPage : MauiControls.TabbedPage
{
    private readonly AppStateService _appState;
    private readonly IServiceProvider _services;

    public NavTabbedPage(AppStateService appState, IServiceProvider services)
    {
        _appState = appState;
        _services = services;
        
        InitializeTabs();

        UpdateBarColors();

        Application.Current!.RequestedThemeChanged += (_, _) => UpdateBarColors();
        
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
    }

    private void UpdateBarColors()
    {
        bool dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        BarBackgroundColor = dark ? Color.FromArgb("#1F1F23") : Color.FromArgb("#F7F7FA");
        BarTextColor = dark ? Colors.White : Colors.Black;
        SelectedTabColor = Colors.MediumPurple;
        UnselectedTabColor = dark ? Color.FromArgb("#F7F7FA") : Color.FromArgb("#1F1F23");
    }

    private void InitializeTabs()
    {
        var mainPage = _services.GetRequiredService<MainPage>();
        mainPage.BindingContext = _appState;
        mainPage.Title = "Главная";
        mainPage.IconImageSource = "home_image.png";
        var mainNav = new MauiControls.NavigationPage(mainPage)
        {
            Title = "Главная",
            IconImageSource = "home_image.png"
        };
        MauiControls.NavigationPage.SetHasNavigationBar(mainPage, false);

        var foldersPage = _services.GetRequiredService<FoldersPage>();
        foldersPage.BindingContext = _appState;
        foldersPage.Title = "Папки";
        foldersPage.IconImageSource = "folder_image.png";
        var foldersNav = new MauiControls.NavigationPage(foldersPage)
        {
            Title = "Папки",
            IconImageSource = "folder_image.png"
        };
        MauiControls.NavigationPage.SetHasNavigationBar(foldersPage, false);

        var settingsPage = _services.GetRequiredService<SettingsPage>();
        settingsPage.BindingContext = _appState;
        settingsPage.Title = "Настройки";
        settingsPage.IconImageSource = "settings_image.png";
        var settingsNav = new MauiControls.NavigationPage(settingsPage)
        {
            Title = "Настройки",
            IconImageSource = "settings_image.png"
        };
        MauiControls.NavigationPage.SetHasNavigationBar(settingsPage, false);

        Children.Add(mainNav);
        Children.Add(foldersNav);
        Children.Add(settingsNav);
    }
}