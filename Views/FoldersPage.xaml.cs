using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackTop.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
namespace TrackTop;

public partial class FoldersPage : ContentPage
{
    public FoldersPage()
    {
        InitializeComponent();
    }

    private async void OnOpenPopupClicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new FolderCreator(),  new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            PageOverlayColor = Color.FromRgba(0, 0, 0, 120)
        });
    }
}