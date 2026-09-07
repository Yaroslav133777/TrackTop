﻿
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
namespace TrackTop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiCommunityToolkit()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter_28pt-Regular.ttf", "Inter");
                fonts.AddFont("Inter_28pt-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter_28pt-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Inter_28pt-Bold.ttf", "InterBold");
            })
            .Services.AddSingleton<AppStateService>()
            .AddTransient<MainPage>()
            .AddTransient<FoldersPage>()
            .AddTransient<SettingsPage>()
            .AddTransient<NavTabbedPage>(sp => new NavTabbedPage(
                sp.GetRequiredService<AppStateService>(), 
                sp));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
        int a = 10;
    }
}