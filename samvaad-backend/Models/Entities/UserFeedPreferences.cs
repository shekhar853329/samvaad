using samvaad_backend.Models.Enums;

namespace samvaad_backend.Models.Entities;

/// <summary>
/// User preferences for feed content and algorithm (maps to Settings → Feed &amp; content).
/// </summary>
public class UserFeedPreferences
{
    public Guid UserId { get; set; }
    public FeedOrder FeedOrder { get; set; } = FeedOrder.Ranked;
    public bool ShowSuggestedPosts { get; set; } = true;
    public bool ShowTrendingTopics { get; set; } = true;
    public SensitiveContentPolicy SensitiveContent { get; set; } = SensitiveContentPolicy.BlurWithWarning;
    public AutoplayVideos AutoplayVideos { get; set; } = AutoplayVideos.OnWifiOnly;
    public bool ShowLikedPostsInOtherFeeds { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<MutedWord> MutedWords { get; set; } = [];
}
