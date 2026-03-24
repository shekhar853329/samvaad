using System.ComponentModel.DataAnnotations;

namespace samvaad_backend.Models.DTOs.Posts;

public class CreatePostRequest
{
    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    public Guid? ParentPostId { get; set; }

    public Guid? RepostOfId { get; set; }
}
