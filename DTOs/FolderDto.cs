namespace TrackTop;

public class FolderDto
{
    public string Name { get; set; } = "";
    public string? Color { get; set; }
    public List<TaskDto> Tasks { get; set; } = new();
}
