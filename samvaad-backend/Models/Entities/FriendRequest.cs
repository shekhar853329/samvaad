using samvaad_backend.Models.Enums;

namespace samvaad_backend.Models.Entities;

public class FriendRequest
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
