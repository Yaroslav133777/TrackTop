namespace TrackTop.Views;

public partial class FolderDetailPage : ContentPage
{
    private readonly AppStateService _appState;
    private readonly FolderModel _folder;

    public FolderDetailPage(AppStateService appState, FolderModel folder)
    {
        _appState = appState;
        _folder = folder;
        InitializeComponent();
        BindingContext = folder;
        UpdateTasksCount();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateTasksCount();
    }

    private void UpdateTasksCount()
    {
        int count = _folder.Tasks.Count;
        var plural = count % 10 == 1 && count % 100 != 11 ? "задача" :
            count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 12 || count % 100 > 14) ? "задачи" : "задач";
        TasksCountLabel.Text = $"{count} {plural}";
        TasksCollection.ItemsSource = _folder.Tasks.ToList();
        bool hasTasks = count > 0;
        EmptyStateLayout.IsVisible = !hasTasks;
        TasksCollection.IsVisible = hasTasks;
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnDeleteTaskTapped(object? sender, EventArgs e)
    {
        if (sender is Element { BindingContext: TaskModel task })
        {
            _appState.RemoveTask(task);
            UpdateTasksCount();
        }
    }
}