using TrackTop.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
namespace TrackTop;

public partial class FoldersPage : ContentPage
{
    private readonly AppStateService _appState;

    public FoldersPage(AppStateService appState)
    {
        _appState = appState;
        InitializeComponent();
        BindingContext = _appState;

        _appState.Folders.CollectionChanged += (s, e) => UpdateEmptyState();
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        EmptyFolders.IsVisible = _appState.Folders.Count == 0;
    }

    private async void OnFolderTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is FolderModel folder)
        {
            await Navigation.PushAsync(new FolderDetailPage(_appState, folder));
        }
    }

    private void OnDeleteFolderTapped(object? sender, EventArgs e)
    {
        if (sender is Element { BindingContext: FolderModel folder })
            _appState.RemoveFolder(folder);
    }

    private async void OnOpenPopupClicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new FolderCreator(_appState),  new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            PageOverlayColor = Color.FromRgba(0, 0, 0, 120)
        });
    }
}