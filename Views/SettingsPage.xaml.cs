using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using TrackTop.Views;

namespace TrackTop;

public partial class SettingsPage : ContentPage
{
    private readonly AppStateService _appState;

    public SettingsPage(AppStateService appState)
    {
        _appState = appState;
        InitializeComponent();
        BindingContext = appState;

        ThemeSwitch.Toggled += OnThemeToggled;
        NotificationsSwitch.Toggled += OnNotificationsToggled;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ThemeSwitch.Toggled -= OnThemeToggled;
        ThemeSwitch.IsToggled = _appState.DarkTheme;
        ThemeSwitch.Toggled += OnThemeToggled;
        UpdateThemeText(_appState.DarkTheme);

        NotificationsSwitch.Toggled -= OnNotificationsToggled;
        NotificationsSwitch.IsToggled = _appState.NotificationsEnabled;
        NotificationsSwitch.Toggled += OnNotificationsToggled;
        UpdateNotificationsText(_appState.NotificationsEnabled);
    }

    private void OnThemeToggled(object? sender, ToggledEventArgs e)
    {
        _appState.DarkTheme = e.Value;
        if (Application.Current != null)
            Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        UpdateThemeText(e.Value);
    }

    private void OnNotificationsToggled(object? sender, ToggledEventArgs e)
    {
        _appState.NotificationsEnabled = e.Value;
        UpdateNotificationsText(e.Value);
    }

    private void UpdateThemeText(bool isDark)
        => ThemeText.Text = isDark ? "Тёмная тема" : "Светлая тема";

    private void UpdateNotificationsText(bool enabled)
        => NotificationsText.Text = enabled ? "Включены" : "Выключены";

    private async void OnProfileTapped(object? sender, TappedEventArgs e)
    {
        await this.ShowPopupAsync(new UsernameEditor(_appState), new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            PageOverlayColor = Color.FromRgba(0, 0, 0, 120)
        });
    }
}