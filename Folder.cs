using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrackTop;

public class Folder : INotifyPropertyChanged
{
    public string FolderName { get; set; } = string.Empty;
    public List<Task> Tasks { get; set; } = new List<Task>();
    public Color FolderColor { get; set; }

    public Folder(int red, int green, int blue)
    {
        FolderColor = new Color(red, green, blue);
    }

    public void AddTask(Task task)
    {
        Tasks.Add(task);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
}