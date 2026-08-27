namespace TrackTop;

public class StorageData
{
    public string? Username { get; set; }
    public List<FolderDto> Folders { get; set; } = new();
    public List<TaskDto> TodayTasks { get; set; } = new();
    public List<TaskDto> PlannedTasks { get; set; } = new();
    public List<TaskDto> NotesTasks { get; set; } = new();
    public List<TaskDto> CompletedTasks { get; set; } = new();
}
