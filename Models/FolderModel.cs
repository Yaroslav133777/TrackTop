using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrackTop;

public class FolderModel : INotifyPropertyChanged
{
    private string _folderName = "default";
    private Color _folderColor;
    private Color _iconBackground;
    private Color _iconColor;

    public string FolderName
    {
        get => _folderName;
        set { _folderName = value; OnPropertyChanged(); }
    }

    public List<TaskModel> Tasks { get; set; } = new List<TaskModel>();
    public Color FolderColor
    {
        get => _folderColor;
        set { _folderColor = value; OnPropertyChanged(); }
    }

    public Color IconBackground
    {
        get => _iconBackground;
        set { _iconBackground = value; OnPropertyChanged(); }
    }

    public Color IconColor
    {
        get => _iconColor;
        set { _iconColor = value; OnPropertyChanged(); }
    }

    public int Count => Tasks.Count;

    public string Title => FolderName;

    public FolderModel(string color)
    {
        FolderColor = Color.FromArgb(color);
        UpdateIconColors();
    }

    public void AddTask(TaskModel taskModel)
    {
        Tasks.Add(taskModel);
        OnPropertyChanged(nameof(Count));
    }

    public void UpdateColor(Color newColor)
    {
        FolderColor = newColor;
        UpdateIconColors();
    }

    private void UpdateIconColors()
    {
        IconBackground = FolderColor.WithAlpha(0.2f);
        IconColor = FolderColor;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}