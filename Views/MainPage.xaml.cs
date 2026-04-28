using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
namespace TrackTop;
public partial class MainPage : ContentPage
{
    private string RecognitionText {get; set;} = string.Empty;
    private readonly ISpeechToText speechToText;
    private CancellationTokenSource? cancellationTokenSource;
    private string RecognizedText { get; set; } = string.Empty;

    public MainPage()
    {
        speechToText = Application.Current?.Handler?.MauiContext?.Services.GetService<ISpeechToText>() ?? SpeechToText.Default;
        InitializeComponent();
        if (DateTime.Now.Hour > 3 && DateTime.Now.Hour < 10)
        {
            MorningText.Text = "Доброе утро!"; //Текст под именем юзера
        }
        bool isWindows = OperatingSystem.IsWindows();
        if (isWindows)
        {
            MainGrid.RowDefinitions[2].Height = 850;
        }
        else
        {
            MainGrid.RowDefinitions[2].Height = GridLength.Star;
        }
    }
    private async void Listen(object sender, PointerEventArgs args)
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
        speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;
        var isGranted = await speechToText.RequestPermissions(cancellationTokenSource.Token);
        if (!isGranted)
        {
            await Toast.Make("Permission not granted").Show(CancellationToken.None);
            return;
        }
        
        await speechToText.StartListenAsync(new SpeechToTextOptions { Culture = CultureInfo.CurrentCulture, ShouldReportPartialResults = true }, cancellationTokenSource.Token);
    }
    private async void StopListening(object? sender, PointerEventArgs args)
    {
        await speechToText.StopListenAsync(CancellationToken.None);
        speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
        speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
    }
    void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        RecognitionText += args.RecognitionResult;
    }

    void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        RecognitionText = args.RecognitionResult.Text;
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