namespace samvaad_backend.Models.Entities;

public class StoryView
{
    public Guid StoryId { get; set; }
    public Guid ViewerId { get; set; }
    public DateTime ViewedAt { get; set; }

    // Navigation properties
    public Story Story { get; set; } = null!;
    public User Viewer { get; set; } = null!;
}
