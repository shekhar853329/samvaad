namespace samvaad_backend.Models.Entities;

public class Follow
{
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    /// <summary>The user who is doing the following.</summary>
    public User Follower { get; set; } = null!;
    /// <summary>The user being followed.</summary>
    public User Following { get; set; } = null!;
}
