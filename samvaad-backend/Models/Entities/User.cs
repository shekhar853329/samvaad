using samvaad_backend.Models.Entities;

namespace samvaad_backend.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public bool IsVerified { get; set; }
    public bool IsPrivate { get; set; }
    public Guid? PinnedPostId { get; set; }
    public int PostsCount { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public long TotalViewsCount { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime JoinedAt { get; set; }

    // Navigation properties
    public Post? PinnedPost { get; set; }
    public ICollection<Post> Posts { get; set; } = [];
    public ICollection<Follow> Following { get; set; } = [];
    public ICollection<Follow> Followers { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<Bookmark> Bookmarks { get; set; } = [];
    public ICollection<Mention> Mentions { get; set; } = [];
    public ICollection<ConversationParticipant> ConversationParticipants { get; set; } = [];
    public ICollection<Message> SentMessages { get; set; } = [];
    public ICollection<Notification> ReceivedNotifications { get; set; } = [];
    public ICollection<Notification> SentNotifications { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Story> Stories { get; set; } = [];
    public ICollection<StoryView> StoryViews { get; set; } = [];
    public ICollection<UserTag> Tags { get; set; } = [];
    public UserNotificationPreferences? NotificationPreferences { get; set; }
    public UserPrivacySettings? PrivacySettings { get; set; }
    public UserFeedPreferences? FeedPreferences { get; set; }
}
