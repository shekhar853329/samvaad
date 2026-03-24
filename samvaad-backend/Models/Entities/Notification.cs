using samvaad_backend.Models.Enums;

namespace samvaad_backend.Models.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public Guid? ActorId { get; set; }
    public NotificationType Type { get; set; }
    public Guid? PostId { get; set; }
    public bool IsRead { get; set; }
    /// <summary>Used for system notifications (e.g. "Your post reached 5,000 views").</summary>
    public string? SystemMessage { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User Recipient { get; set; } = null!;
    public User? Actor { get; set; }
    public Post? Post { get; set; }
}
