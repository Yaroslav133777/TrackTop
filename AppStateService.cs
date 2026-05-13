using System.Collections.ObjectModel;
namespace TrackTop;

public class AppStateService
{
    public string Username { get; set; } = "Ярослав";
    public ObservableCollection<FolderModel> Folders { get; set; } = new();
    public ObservableCollection<TaskModel> TodayTasks { get; set; } = new();
    public ObservableCollection<TaskModel> PlannedTasks { get; set; } = new();
    public ObservableCollection<TaskModel> NotesTasks { get; set; } = new();
}