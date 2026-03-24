namespace samvaad_backend.Models.Entities;

public class Post
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentPostId { get; set; }
    public Guid? RepostOfId { get; set; }
    public bool IsPinned { get; set; }
    public bool IsLive { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public int RepostsCount { get; set; }
    public int ViewsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Author { get; set; } = null!;
    public Post? ParentPost { get; set; }
    public Post? RepostOf { get; set; }
    public ICollection<Post> Replies { get; set; } = [];
    public ICollection<Post> Reposts { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<Bookmark> Bookmarks { get; set; } = [];
    public ICollection<PostHashTag> PostHashTags { get; set; } = [];
    public ICollection<PostMedia> Media { get; set; } = [];
    public ICollection<Mention> Mentions { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
