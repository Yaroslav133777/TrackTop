namespace TrackTop;
using MauiIcons.Core;
using MauiIcons.Material;
public partial class MainPage : ContentPage
{
    
    public MainPage()
    {
        InitializeComponent();
        if (DateTime.Now.Hour > 3 && DateTime.Now.Hour < 10)
        {
            MorningText.Text = "Доброе утро!"; //Текст под именем юзера
        }
    }
    private void OnAvatarClicked(object? sender, EventArgs e)
    {
        if (Parent is NavTabbedPage navPage)
        {
            navPage.CurrentPage = navPage.Children[2];
        }
    }
    private void ThemeSwitcher(object? sender, EventArgs e)
    {
        if (App.Current.RequestedTheme == AppTheme.Dark)
        {
            App.Current.UserAppTheme = AppTheme.Light;
        }
        else if (App.Current.RequestedTheme == AppTheme.Light)
        {
            App.Current.UserAppTheme = AppTheme.Dark;
        }
    }
}