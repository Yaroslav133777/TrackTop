using Microsoft.Maui.Platform;

namespace TrackTop;

public partial class App
{
    private readonly AppStateService _appState;
    private readonly IServiceProvider _services;

    public App(AppStateService appState, IServiceProvider services)
    {
        _appState = appState;
        _services = services;
        InitializeComponent();
        UserAppTheme = appState.DarkTheme ? AppTheme.Dark : AppTheme.Light;
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navPage = _services.GetRequiredService<NavTabbedPage>();
        var appState = _appState;

        var window = new Window(navPage);
        ApplyWindowBackground(window);
        RequestedThemeChanged += (_, _) =>
        {
            foreach (var w in Windows)
                ApplyWindowBackground(w);
        };
        window.Stopped += (_, _) => appState.SaveData();
        window.Destroying += (_, _) => appState.SaveData();

#if WINDOWS || MACCATALYST
        window.MinimumWidth = 850;  
        window.MinimumHeight = 620;    
        
        window.Width = 1100;
        window.Height = 720;
#endif

        return window;
    }

    private static void ApplyWindowBackground(Window window)
    {
#if IOS
        var color = (Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#1F1F23")
            : Color.FromArgb("#F7F7FA")).ToPlatform();

        if (window.Handler?.PlatformView is UIKit.UIWindow uiWindow)
            uiWindow.BackgroundColor = color;

        window.HandlerChanged += (_, _) =>
        {
            if (window.Handler?.PlatformView is UIKit.UIWindow w)
                w.BackgroundColor = color;
        };
#endif
    }
}