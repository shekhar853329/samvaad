namespace samvaad_backend.Models.Entities;

public class PostHashTag
{
    public Guid PostId { get; set; }
    public Guid HashTagId { get; set; }

    // Navigation properties
    public Post Post { get; set; } = null!;
    public HashTag HashTag { get; set; } = null!;
}
