using Microsoft.Extensions.DependencyInjection;

namespace TrackTop;

public partial class App : Application
{
    public static AppStateService AppState { get; private set; } = new AppStateService();
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Unspecified;
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new NavTabbedPage());

#if WINDOWS
        window.MinimumWidth = 850;  
        window.MinimumHeight = 620;    
        
        window.Width = 1100;
        window.Height = 720;
#endif

        return window;
    }
}