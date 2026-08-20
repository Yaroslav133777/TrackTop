using Microsoft.Extensions.DependencyInjection;

namespace TrackTop;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Unspecified;
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var services = Handler?.MauiContext?.Services ?? Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;
        
        var appState = services?.GetService<AppStateService>() ?? new AppStateService();

        UserAppTheme = appState.DarkTheme ? AppTheme.Dark : AppTheme.Light;

        var navPage = services?.GetService<NavTabbedPage>()
            ?? new NavTabbedPage(appState, services ?? new ServiceCollection().BuildServiceProvider());

        var window = new Window(navPage);
        window.Stopped += (_, _) => appState.SaveData();
        window.Destroying += (_, _) => appState.SaveData();

#if WINDOWS
        window.MinimumWidth = 850;  
        window.MinimumHeight = 620;    
        
        window.Width = 1100;
        window.Height = 720;
#endif

        return window;
    }
}