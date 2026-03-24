namespace samvaad_backend.Models.Entities;

public class HashTag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int PostCount { get; set; }

    // Navigation properties
    public ICollection<PostHashTag> PostHashTags { get; set; } = [];
}
