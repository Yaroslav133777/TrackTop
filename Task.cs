namespace TrackTop;

public class Task
{
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? Time { get; set; }
    public bool Notify {get; set;}

    public Task(string? description, DateOnly date, TimeOnly? time, bool notify)
    {
        Description = description;
        Date = date;
        Time = time;
        Notify = notify;
    }
    
}