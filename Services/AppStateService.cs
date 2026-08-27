using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Storage;

namespace TrackTop;

public class AppStateService : INotifyPropertyChanged
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private string DataFilePath => Path.Combine(FileSystem.AppDataDirectory, "tracktop_data.json");

    private string _username = "Username";

    public string Username
    {
        get => _username;
        set
        {
            if (_username == value)
                return;
            _username = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UsernameFirstLetter));
        }
    }

    public string UsernameFirstLetter => string.IsNullOrEmpty(Username) ? "?" : $"{Username[0]}";

    public ObservableCollection<FolderModel> Tasks { get; set; } = new();
    public ObservableCollection<FolderModel> Folders { get; set; } = new();
    public ObservableCollection<TaskModel> TodayTasks { get; set; } = new();
    public ObservableCollection<TaskModel> PlannedTasks { get; set; } = new();
    public ObservableCollection<TaskModel> NotesTasks { get; set; } = new();
    public ObservableCollection<TaskModel> CompletedTasks { get; set; } = new();

    public IReadOnlyList<TaskModel> AllTasks
    {
        get
        {
            var result = new List<TaskModel>(TodayTasks);
            foreach (var t in PlannedTasks)
                if (!result.Contains(t))
                    result.Add(t);
            foreach (var t in NotesTasks)
                if (!result.Contains(t))
                    result.Add(t);
            foreach (var folder in Folders)
                foreach (var t in folder.Tasks)
                    if (!result.Contains(t))
                        result.Add(t);
            return result;
        }
    }

    public bool NotificationsEnabled
    {
        get => Preferences.Default.Get(nameof(NotificationsEnabled), false);
        set => Preferences.Default.Set(nameof(NotificationsEnabled), value);
    }

    public bool DarkTheme
    {
        get => Preferences.Default.Get(nameof(DarkTheme), false);
        set => Preferences.Default.Set(nameof(DarkTheme), value);
    }

    public AppStateService()
    {
        LoadData();
        MoveExpiredTasksToCompleted();
        SaveData();
        Folders.CollectionChanged += (_, _) => SaveData();
        TodayTasks.CollectionChanged += (_, _) => SaveData();
        PlannedTasks.CollectionChanged += (_, _) => SaveData();
        NotesTasks.CollectionChanged += (_, _) => SaveData();
        CompletedTasks.CollectionChanged += (_, _) => SaveData();
    }

    public void RemoveTask(TaskModel task)
    {
        TodayTasks.Remove(task);
        PlannedTasks.Remove(task);
        NotesTasks.Remove(task);
        CompletedTasks.Remove(task);
        foreach (var folder in Folders)
            folder.Tasks.Remove(task);
        SaveData();
    }

    public void RemoveFolder(FolderModel folder)
    {
        foreach (var task in folder.Tasks.ToList())
            RemoveTask(task);
        Folders.Remove(folder);
        SaveData();
    }

    private void MoveExpiredTasksToCompleted()
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);

        foreach (var task in TodayTasks.Where(t => IsExpired(t, today, now)).ToList())
        {
            TodayTasks.Remove(task);
            if (!CompletedTasks.Contains(task))
                CompletedTasks.Add(task);
        }

        foreach (var task in PlannedTasks.Where(t => IsExpired(t, today, now)).ToList())
        {
            PlannedTasks.Remove(task);
            if (!CompletedTasks.Contains(task))
                CompletedTasks.Add(task);
        }

        foreach (var task in NotesTasks.Where(t => IsExpired(t, today, now)).ToList())
        {
            NotesTasks.Remove(task);
            if (!CompletedTasks.Contains(task))
                CompletedTasks.Add(task);
        }

        foreach (var folder in Folders)
        {
            foreach (var task in folder.Tasks.Where(t => IsExpired(t, today, now)).ToList())
            {
                folder.Tasks.Remove(task);
                if (!CompletedTasks.Contains(task))
                    CompletedTasks.Add(task);
            }
        }
    }

    private static bool IsExpired(TaskModel task, DateOnly today, DateTime now)
    {
        if (task.Date < today)
            return true;
        if (task.Date > today)
            return false;
        if (task.Time is { } time)
            return time <= TimeOnly.FromDateTime(now);
        return false;
    }

    public void SaveData()
    {
        try
        {
            var data = new StorageData
            {
                Username = Username,
                Folders = Folders.Select(ToDto).ToList(),
                TodayTasks = TodayTasks.Select(ToDto).ToList(),
                PlannedTasks = PlannedTasks.Select(ToDto).ToList(),
                NotesTasks = NotesTasks.Select(ToDto).ToList(),
                CompletedTasks = CompletedTasks.Select(ToDto).ToList()
            };
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(DataFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AppStateService.SaveData error: {ex}");
        }
    }

    private void LoadData()
    {
        try
        {
            if (!File.Exists(DataFilePath))
                return;

            var json = File.ReadAllText(DataFilePath);
            var data = JsonSerializer.Deserialize<StorageData>(json, JsonOptions);
            if (data == null)
                return;

            if (!string.IsNullOrWhiteSpace(data.Username))
                Username = data.Username;

            var folderByName = new Dictionary<string, FolderModel>();
            foreach (var dto in data.Folders)
            {
                var folder = new FolderModel(string.IsNullOrWhiteSpace(dto.Color) ? "#6B5CF6" : dto.Color)
                {
                    FolderName = dto.Name
                };
                folderByName[dto.Name] = folder;
                Folders.Add(folder);
            }

            foreach (var dto in data.Folders)
            {
                var folder = folderByName[dto.Name];
                foreach (var taskDto in dto.Tasks)
                {
                    var task = ToModel(taskDto, folderByName);
                    if (task != null)
                        folder.Tasks.Add(task);
                }
            }

            foreach (var dto in data.TodayTasks)
            {
                var task = ToModel(dto, folderByName);
                if (task != null)
                    TodayTasks.Add(task);
            }

            foreach (var dto in data.PlannedTasks)
            {
                var task = ToModel(dto, folderByName);
                if (task != null)
                    PlannedTasks.Add(task);
            }

            foreach (var dto in data.NotesTasks)
            {
                var task = ToModel(dto, folderByName);
                if (task != null)
                    NotesTasks.Add(task);
            }

            foreach (var dto in data.CompletedTasks)
            {
                var task = ToModel(dto, folderByName);
                if (task != null)
                    CompletedTasks.Add(task);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AppStateService.LoadData error: {ex}");
        }
    }

    private static FolderDto ToDto(FolderModel folder) => new()
    {
        Name = folder.FolderName,
        Color = folder.FolderColor.ToArgbHex(),
        Tasks = folder.Tasks.Select(ToDto).ToList()
    };

    private static TaskDto ToDto(TaskModel task) => new()
    {
        Description = task.Description,
        Date = task.Date.ToString("yyyy-MM-dd"),
        Time = task.Time?.ToString("HH:mm"),
        Notify = task.Notify,
        FolderName = task.Folder?.FolderName
    };

    private static TaskModel? ToModel(TaskDto dto, Dictionary<string, FolderModel> folderByName)
    {
        if (string.IsNullOrWhiteSpace(dto.Description))
            return null;
        if (!DateOnly.TryParse(dto.Date, out var date))
            return null;

        TimeOnly? time = null;
        if (!string.IsNullOrWhiteSpace(dto.Time) && TimeOnly.TryParse(dto.Time, out var t))
            time = t;

        FolderModel? folder = null;
        if (!string.IsNullOrWhiteSpace(dto.FolderName) && folderByName.TryGetValue(dto.FolderName, out var f))
            folder = f;

        return new TaskModel(dto.Description, date, time, dto.Notify, folder);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
