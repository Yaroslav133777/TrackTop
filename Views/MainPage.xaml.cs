using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using TrackTop.Views;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls;

namespace TrackTop;
public partial class MainPage : ContentPage
{
    private readonly AppStateService _appState;
    private Button? _selectedButton;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _manuallyStopped = false;
    private string _recognizedText = string.Empty;
    private bool _isRecording = false;
    private bool _speechUnsupported = false;

    public MainPage(AppStateService appState)
    {
        _appState = appState;
        InitializeComponent();
        
        if (Application.Current?.RequestedTheme == AppTheme.Light)
            DarkModeButton.Source = "sunny.png";
        
        _selectedButton = TodayButton;
        BindingContext = _appState;
        
        _appState.TodayTasks.CollectionChanged += (s, e) => ApplyFilter();
        _appState.PlannedTasks.CollectionChanged += (s, e) => ApplyFilter();
        _appState.NotesTasks.CollectionChanged += (s, e) => ApplyFilter();
        _appState.Folders.CollectionChanged += (s, e) => ApplyFilter();
        ApplyFilter();
    }

    public Color ThemeIconColor =>
        Application.Current?.RequestedTheme == AppTheme.Dark
            ? Colors.White 
            : Colors.Black;

    public Color ThemeTextColor =>
        Application.Current?.RequestedTheme == AppTheme.Dark
            ? Colors.White
            : Colors.Black;

    private void ApplyFilter()
    {
        var all = _appState.AllTasks;
        List<TaskModel> filtered;

        if (_selectedButton == TodayButton)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            filtered = all.Where(t => t.Date == today).ToList();
            EmptyTitleLabel.Text = "Нет задач на сегодня";
            EmptySubtitleLabel.Text = "На сегодня задач нет. Отличный день!";
        }
        else if (_selectedButton == NotesButton)
        {
            filtered = all.Where(t => t.Folder == null).ToList();
            EmptyTitleLabel.Text = "Нет заметок";
            EmptySubtitleLabel.Text = "Заметок пока нет. Создайте задачу без папки";
        }
        else
        {
            filtered = all.ToList();
            EmptyTitleLabel.Text = "Нет задач";
            EmptySubtitleLabel.Text = "Запланированных задач пока нет";
        }

        TasksCollection.ItemsSource = filtered;
        bool hasTasks = filtered.Count > 0;
        EmptyStateLayout.IsVisible = !hasTasks;
        TasksCollection.IsVisible = hasTasks;
    }

    private async void Listen(object? sender, EventArgs args)
    {
        if (_isRecording || _speechUnsupported)
            return;
        
        _recognizedText = string.Empty;
        _manuallyStopped = false;
        
        try
        {
            var micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (micStatus != PermissionStatus.Granted)
            {
                micStatus = await Permissions.RequestAsync<Permissions.Microphone>();
                if (micStatus != PermissionStatus.Granted)
                {
                    await Toast.Make("Доступ к микрофону не разрешён").Show(CancellationToken.None);
                    return;
                }
            }
            
            var stt = SpeechToText.Default;
            
            stt.StateChanged -= OnSpeechStateChanged;
            stt.RecognitionResultCompleted -= OnRecognitionTextCompleted;
            stt.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            
            stt.StateChanged += OnSpeechStateChanged;
            stt.RecognitionResultCompleted += OnRecognitionTextCompleted;
            stt.RecognitionResultUpdated += OnRecognitionTextUpdated;
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            _isRecording = true;
            await stt.StartListenAsync(new SpeechToTextOptions 
            { 
                Culture = CultureInfo.CurrentCulture, 
                ShouldReportPartialResults = false
            }, _cancellationTokenSource.Token);
            
            MicButton.BackgroundColor = Colors.Red;
            MicButton.Source = "stop_image.png";
            MicButton.Clicked -= Listen;
            MicButton.Clicked += StopListening;
        }
        catch (Exception ex)
        {
            _isRecording = false;
            _speechUnsupported = true;
            Console.WriteLine($"Voice exception: {ex}");
            try
            {
                await Toast.Make("Голосовой ввод недоступен на этом устройстве").Show(CancellationToken.None);
            }
            catch
            {
            }
        }
    }
    
    private async void StopListening(object? sender, EventArgs args)
    {
        _manuallyStopped = true;
        
        try
        {
            var stt = SpeechToText.Default;
            await stt.StopListenAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Stop error: {ex}");
        }
        
        await Task.Delay(300);
        CleanupSpeechToText();
    }

    private void CleanupSpeechToText()
    {
        var stt = SpeechToText.Default;
        stt.StateChanged -= OnSpeechStateChanged;
        stt.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        stt.RecognitionResultUpdated -= OnRecognitionTextUpdated;
        
        _isRecording = false;
        ResetMicButton();
    }
    
    private void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        try
        {
            _ = Toast.Make(args.RecognitionResult).Show(CancellationToken.None);
        }
        catch
        {
        }
    }
    
    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        _recognizedText = args.RecognitionResult.Text;
    }
    
    private async void OnSpeechStateChanged(object? sender, SpeechToTextStateChangedEventArgs e)
    {
        try
        {
            if (e.State == SpeechToTextState.Silence || e.State == SpeechToTextState.Stopped)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    CleanupSpeechToText();
                });
                
                if (!string.IsNullOrWhiteSpace(_recognizedText) && !_manuallyStopped)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await OpenTaskCreatorWithVoiceInput(_recognizedText);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SpeechState error: {ex}");
        }
    }

    private void ResetMicButton()
    {
        MicButton.Clicked += Listen;
        MicButton.Clicked -= StopListening;
        MicButton.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb("#FF512BD4"), Color.FromArgb("#FFAC99EA"));
        MicButton.Source = "mic_image.png";
    }
    
    private async Task OpenTaskCreatorWithVoiceInput(string voiceText)
    {
        var taskCreator = new TaskCreator(_appState);
        taskCreator.SetVoiceText(voiceText);
        await this.ShowPopupAsync(taskCreator, new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            PageOverlayColor = Color.FromRgba(0, 0, 0, 120)
        });
    }

    private void OnAvatarClicked(object? sender, EventArgs e)
    {
        Element? current = this;
        while (current != null)
        {
            if (current is NavTabbedPage navPage)
            {
                navPage.CurrentPage = navPage.Children[2];
                return;
            }
            current = current.Parent;
        }
    }
    
    private void ThemeSwitcher(object? sender, EventArgs e)
    {
        if (App.Current.RequestedTheme == AppTheme.Dark)
        {
            App.Current.UserAppTheme = AppTheme.Light;
            if (sender is ImageButton button)
                button.Source = "sunny.png";
            _appState.DarkTheme = false;
        }
        else if (App.Current.RequestedTheme == AppTheme.Light)
        {
            App.Current.UserAppTheme = AppTheme.Dark;
            if (sender is ImageButton button)
                button.Source = "dark_mode.png";
            _appState.DarkTheme = true;
        }
    }
    
    private async void OnOpenPopupClicked(object sender, EventArgs e)
    {
        var taskCreator = new TaskCreator(_appState);
        await this.ShowPopupAsync(taskCreator, new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            PageOverlayColor = Color.FromRgba(0, 0, 0, 120)
        });
    }

    private void SelectButton(object? sender, EventArgs e)
    {
        if (sender is not Button button)
            return;
        if (_selectedButton == button)
            return;

        if (_selectedButton != null)
        {
            var prev = _selectedButton;
            prev.SetAppThemeColor(BackgroundColorProperty, Colors.White, Color.FromArgb("#2B2B31"));
            prev.SetAppThemeColor(Button.TextColorProperty, Color.FromArgb("#4B4B55"), Colors.White);
            prev.SetAppThemeColor(Button.BorderColorProperty, Color.FromArgb("#E4E4E8"), Color.FromArgb("#3B3B42"));
            prev.BorderWidth = 1;
        }

        _selectedButton = button;
        _selectedButton.BackgroundColor = Color.FromRgba("#5B4CF0");
        _selectedButton.BorderColor = Color.FromRgba("#5B4CF0");
        _selectedButton.BorderWidth = 0;
        _selectedButton.TextColor = Colors.White;

        ApplyFilter();
    }
}