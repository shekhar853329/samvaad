namespace samvaad_backend.Models.Entities;

public class MutedWord
{
    public Guid UserId { get; set; }
    public string Word { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public UserFeedPreferences FeedPreferences { get; set; } = null!;
}
