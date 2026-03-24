namespace samvaad_backend.Models.Entities;

public class UserTag
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public User User { get; set; } = null!;
}
