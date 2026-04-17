using Microsoft.Extensions.Logging;
using MauiIcons.Fluent;
using MauiIcons.Fluent.Filled;
using MauiIcons.Material;
namespace TrackTop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseFluentMauiIcons()
            .UseFluentFilledMauiIcons()
            .UseMaterialMauiIcons()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter_28pt-Regular.ttf", "Inter");
                fonts.AddFont("Inter_28pt-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter_28pt-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Inter_28pt-Bold.ttf", "InterBold");
            })
            .Services.AddSingleton<NavTabbedPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}