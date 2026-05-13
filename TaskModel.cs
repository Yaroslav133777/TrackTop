namespace TrackTop;

public class TaskModel
{
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? Time { get; set; }
    public bool Notify {get; set;}

    public TaskModel(string? description, DateOnly date, TimeOnly? time, bool notify)
    {
        Description = description;
        Date = date;
        Time = time;
        Notify = notify;
    }
    
}