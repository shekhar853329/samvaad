namespace samvaad_backend.Models.Entities;

/// <summary>
/// Adjacency-list friendship record. Two rows exist per friendship:
///   (UserA, UserB) and (UserB, UserA).
/// This gives O(log n) single-column index lookups instead of an OR scan
/// across SenderId/ReceiverId on the FriendRequests table.
/// </summary>
public class Friendship
{
    public Guid UserId { get; set; }
    public Guid FriendId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public User Friend { get; set; } = null!;
}
