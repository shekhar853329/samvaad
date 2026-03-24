namespace samvaad_backend.Models.Entities;

public class Story
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public int ViewsCount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User Author { get; set; } = null!;
    public ICollection<StoryView> Views { get; set; } = [];
}
