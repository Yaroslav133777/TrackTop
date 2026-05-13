using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrackTop;

public class FolderModel : INotifyPropertyChanged
{
    public string FolderName { get; set; } = "default";
    public List<TaskModel> Tasks { get; set; } = new List<TaskModel>();
    public Color FolderColor { get; set; }

    public FolderModel(string color)
    {
        FolderColor = Color.FromArgb(color);
    }

    public void AddTask(TaskModel taskModel)
    {
        Tasks.Add(taskModel);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
}