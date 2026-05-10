using System.Globalization;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using TrackTop.Views;
using CommunityToolkit.Maui.Extensions;
namespace TrackTop;
public partial class MainPage : ContentPage
{
    private readonly ISpeechToText speechToText;
    private CancellationTokenSource? cancellationTokenSource;
    private bool manuallyStopped = false;
    private string RecognizedText { get; set; }

    public MainPage()
    {
        speechToText = Application.Current?.Handler?.MauiContext?.Services.GetService<ISpeechToText>() ?? SpeechToText.Default;
        InitializeComponent();
        if (DateTime.Now.Hour > 3 && DateTime.Now.Hour < 10)
        {
            MorningText.Text = "Доброе утро!"; //Текст под именем юзера
        }
        bool isWindows = OperatingSystem.IsWindows();
    }
    private async void Listen(object? sender, EventArgs args)
    {
        RecognizedText = string.Empty;
        manuallyStopped = false;
        speechToText.StateChanged -= OnSpeechStateChanged;
        speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        speechToText.StateChanged += OnSpeechStateChanged;
        speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;
        speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
        try
        {
            var isGranted = await speechToText.RequestPermissions(cancellationTokenSource.Token);
            if (!isGranted)
            {
                await Toast.Make("Permission not granted").Show(CancellationToken.None);
                return;
            }
            await speechToText.StartListenAsync(new SpeechToTextOptions { Culture = CultureInfo.CurrentCulture, ShouldReportPartialResults = false}, cancellationTokenSource.Token);
            MicButton.BackgroundColor = Colors.Red;
            MicButton.Source = "stop_image.png";
            MicButton.Clicked -= Listen;
            MicButton.Clicked += StopListening;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception is: {ex.Message}");
        }
    }
    private async void StopListening(object? sender, EventArgs args)
    {
        manuallyStopped = true;
        await speechToText.StopListenAsync(CancellationToken.None);
        speechToText.StateChanged -= OnSpeechStateChanged;
        speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        MicButton.Clicked += Listen;
        MicButton.Clicked -= StopListening;
        MicButton.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb("#FF512BD4"), Color.FromArgb("#FFAC99EA"));
        MicButton.Source = "mic_image.png";
        
    }
    private async void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        await Toast.Make($"{args.RecognitionResult}").Show(CancellationToken.None);
    }
    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        RecognizedText = args.RecognitionResult.Text;
    }
    private async void OnSpeechStateChanged(object? sender, SpeechToTextStateChangedEventArgs e)
    {
        if (e.State == SpeechToTextState.Silence || e.State == SpeechToTextState.Stopped)
        {
            MicButton.Clicked += Listen;
            MicButton.Clicked -= StopListening;
            MicButton.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb("#FF512BD4"), Color.FromArgb("#FFAC99EA"));
            MicButton.Source = "mic_image.png";
            await Toast.Make($"Запись остановлена").Show(CancellationToken.None);
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
    private async void OnOpenPopupClicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new TaskCreator(),  new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            PageOverlayColor = Color.FromRgba(0, 0, 0, 120)
        });
    }
    
}