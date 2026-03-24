using samvaad_backend.Models.Enums;

namespace samvaad_backend.Models.Entities;

public class UserPrivacySettings
{
    public Guid UserId { get; set; }
    public AccountVisibility AccountVisibility { get; set; } = AccountVisibility.Public;
    public InteractionScope WhoCanMessage { get; set; } = InteractionScope.Everyone;
    public InteractionScope WhoCanSeeFollowers { get; set; } = InteractionScope.Everyone;
    public bool AllowTagging { get; set; } = true;
    public bool AllowReposts { get; set; } = true;
    public bool ShowInSearch { get; set; } = true;
    public bool ShowActivityStatus { get; set; }
    public bool PersonalisedRecommendations { get; set; } = true;
    public bool ShareDataWithPartners { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
