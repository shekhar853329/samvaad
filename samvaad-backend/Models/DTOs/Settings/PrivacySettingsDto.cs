namespace samvaad_backend.Models.DTOs.Settings;

public record PrivacySettingsDto(
    string AccountVisibility,
    string WhoCanMessage,
    string WhoCanSeeFollowers,
    bool AllowTagging,
    bool AllowReposts,
    bool ShowInSearch,
    bool ShowActivityStatus,
    bool PersonalisedRecommendations,
    bool ShareDataWithPartners
);

public class SavePrivacySettingsRequest
{
    /// <summary>"Public" or "Private"</summary>
    public string AccountVisibility { get; set; } = "Public";
    /// <summary>"Everyone", "FollowersOnly", or "Nobody"</summary>
    public string WhoCanMessage { get; set; } = "Everyone";
    /// <summary>"Everyone", "FollowersOnly", or "OnlyMe"</summary>
    public string WhoCanSeeFollowers { get; set; } = "Everyone";
    public bool AllowTagging { get; set; }
    public bool AllowReposts { get; set; }
    public bool ShowInSearch { get; set; }
    public bool ShowActivityStatus { get; set; }
    public bool PersonalisedRecommendations { get; set; }
    public bool ShareDataWithPartners { get; set; }
}
