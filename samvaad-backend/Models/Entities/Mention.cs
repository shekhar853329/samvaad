namespace samvaad_backend.Models.Entities;

public class Mention
{
    public Guid PostId { get; set; }
    public Guid MentionedUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Post Post { get; set; } = null!;
    public User MentionedUser { get; set; } = null!;
}
