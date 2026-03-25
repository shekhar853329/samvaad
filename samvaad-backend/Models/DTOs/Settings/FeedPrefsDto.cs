namespace samvaad_backend.Models.DTOs.Settings;

public record FeedPrefsDto(
    string FeedOrder,
    bool ShowSuggestedPosts,
    bool ShowTrendingTopics,
    string SensitiveContent,
    string AutoplayVideos,
    bool ShowLikedPostsInOtherFeeds,
    IReadOnlyList<string> MutedWords
);

public class SaveFeedPrefsRequest
{
    /// <summary>"Ranked" or "Chronological"</summary>
    public string FeedOrder { get; set; } = "Ranked";
    public bool ShowSuggestedPosts { get; set; }
    public bool ShowTrendingTopics { get; set; }
    /// <summary>"Hide", "BlurWithWarning", or "Show"</summary>
    public string SensitiveContent { get; set; } = "BlurWithWarning";
    /// <summary>"OnWifiOnly", "Always", or "Never"</summary>
    public string AutoplayVideos { get; set; } = "OnWifiOnly";
    public bool ShowLikedPostsInOtherFeeds { get; set; }
}

public class AddMutedWordRequest
{
    public string Word { get; set; } = string.Empty;
}
