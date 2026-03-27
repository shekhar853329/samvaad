namespace samvaad_backend.Models.Entities;

public class Society
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid CreatedByUserId { get; set; }
    public int MembersCount { get; set; }
    public int PostsCount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public ICollection<SocietyMember> Members { get; set; } = [];
    public ICollection<Post> Posts { get; set; } = [];
}
