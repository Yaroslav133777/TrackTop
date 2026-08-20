using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrackTop;

public class TaskModel : INotifyPropertyChanged
{
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? Time { get; set; }
    public bool Notify { get; set; }
    public FolderModel? Folder { get; set; }
    public string? Title => Description?.Split('\n')[0] ?? "";
    public bool HasTag => Folder != null;
    public string Tag => Folder?.FolderName ?? "";
    public string TimeString => Time?.ToString("HH:mm") ?? "";

    public TaskModel(string? description, DateOnly date, TimeOnly? time, bool notify, FolderModel? folder = null)
    {
        Description = description;
        Date = date;
        Time = time;
        Notify = notify;
        Folder = folder;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}