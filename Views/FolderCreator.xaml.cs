using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;

namespace TrackTop.Views;
public partial class FolderCreator : Popup
{
    private readonly AppStateService _appState;
    private Border? _selectedBorder;
    public Color? SelectedColor { get; private set; }

    public FolderCreator(AppStateService? appState = null)
    {
        _appState = appState ?? Application.Current?.Handler?.MauiContext?.Services.GetService<AppStateService>() ?? new AppStateService();
        InitializeComponent();
        CreateColorOptions();
        BindingContext = this;
    }

    private void CreateColorOptions()
    {
        int colorsCounter = 0;
        var colors = new List<Color>
        {
            Color.FromArgb("#EF5350"), Color.FromArgb("#FF9800"), 
            Color.FromArgb("#FFCC00"), Color.FromArgb("#66CC66"), 
            Color.FromArgb("#1DE4BD"), Color.FromArgb("#40C4FF"),
            Color.FromArgb("#4285F4"), Color.FromArgb("#AB47BC"),
            Color.FromArgb("#EC407A"), Color.FromArgb("#EF5350"),
            Color.FromArgb("#757575")
        };

        foreach (var color in colors)
        {
            colorsCounter++;
            var border = new Border
            {
                BackgroundColor = color,
                WidthRequest = 36,
                HeightRequest = 36,
                StrokeThickness = 0,
                StrokeShape = new Ellipse(),
                Margin = new Thickness(2),
                Shadow = new Shadow
                {
                    Brush = Brush.Black,
                    Offset = new Point(0, 1),
                    Opacity = 0.2f,
                    Radius = 4
                }
            };
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (a,b) => SelectColor(border, color);
            border.GestureRecognizers.Add(tapGesture);
            if (colorsCounter <= 6)
            {
                ColorsContainer.Children.Add(border);
            }
            else if (colorsCounter <= 12)
            {
                ColorsContainer2.Children.Add(border);
            }
            
            if (ColorsContainer.Children.Count == 1)
                SelectColor(border, color);
        }
    }

    private async void SelectColor(Border border, Color color)
    {
        // Animate previous selection out
        if (_selectedBorder != null && _selectedBorder != border)
        {
            await _selectedBorder.ScaleTo(1.0, 150, Easing.CubicOut);
            _selectedBorder.StrokeThickness = 0;
            _selectedBorder.Stroke = null;
        }

        // Animate new selection in
        _selectedBorder = border;
        _selectedBorder.StrokeThickness = 4;
        _selectedBorder.Stroke = Colors.White;
        await _selectedBorder.ScaleTo(1.15, 150, Easing.CubicOut);
        await _selectedBorder.ScaleTo(1.0, 150, Easing.CubicOut);

        SelectedColor = color;
    }
    
    private async void ClosePage(object? sender, EventArgs args)
    {
        await CloseAsync();
    }

    private async void OnCreateClicked(object? sender, EventArgs args)
    {
        if (!CanAddFolder()) return;
        
        var folder = new FolderModel(SelectedColor!.ToArgbHex())
        {
            FolderName = NameEntry.Text?.Trim() ?? "Новая папка"
        };
        
        _appState.Folders.Add(folder);
        
        _appState.SaveData();
        
        await CloseAsync();
    }

    private bool CanAddFolder()
    {
        return !string.IsNullOrWhiteSpace(NameEntry.Text) && SelectedColor != null;
    }
}
