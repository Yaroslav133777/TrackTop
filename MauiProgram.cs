using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Media;
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
            .Services.AddSingleton<NavTabbedPage>()
            .AddSingleton<ISpeechToText>(SpeechToText.Default);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}