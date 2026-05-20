using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;

namespace TrackTop.Views;
public partial class FolderCreator : Popup
{
    private ICommand AddFolderCommand { get; set; }
    public FolderCreator()
    {
        AddFolderCommand = new Command(x => AddFolder(SelectedColor.ToArgbHex()), x=> CanAddFolder());
        InitializeComponent();
        CreateColorOptions();
        // BindingContext = new AppStateService();
    }
    private Border? _selectedBorder;
    public Color SelectedColor { get; private set; }

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

    private void SelectColor(Border border, Color color)
    {
        // Снимаем выделение с предыдущего
        if (_selectedBorder != null)
        {
            _selectedBorder.StrokeThickness = 0;
            _selectedBorder.Stroke = null;
        }

        // Выделяем новый
        _selectedBorder = border;
        _selectedBorder.StrokeThickness = 4;
        _selectedBorder.Stroke = Colors.White;

        SelectedColor = color;
    }
    
    private async void ClosePage(object? sender, EventArgs args)
    {
        await CloseAsync();
    }

    private void AddFolder(string color)
    {
        AppStateService ast = new AppStateService();
        ast.Folders.Add(new FolderModel(color));
    }

    private bool CanAddFolder()
    {
        return !string.IsNullOrEmpty(NameEntry.Text);
    }

}
