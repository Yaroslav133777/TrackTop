using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace TrackTop.Views;

public partial class UsernameEditor : Popup
{
    private readonly AppStateService _appState;

    public UsernameEditor(AppStateService appState)
    {
        _appState = appState;
        InitializeComponent();
        NameEntry.Text = _appState.Username;
    }

    private async void ClosePage(object? sender, EventArgs args)
    {
        await CloseAsync();
    }

    private async void OnSaveClicked(object? sender, EventArgs args)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            await Toast.Make("Имя не может быть пустым!", ToastDuration.Long, 15).Show();
            return;
        }

        _appState.Username = name;
        _appState.SaveData();

        await Toast.Make("Имя сохранено!", ToastDuration.Short, 15).Show();
        await CloseAsync();
    }
}