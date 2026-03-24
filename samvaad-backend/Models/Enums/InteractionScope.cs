namespace samvaad_backend.Models.Enums;

/// <summary>Used for WhoCanMessage and WhoCanSeeFollowers settings.</summary>
public enum InteractionScope
{
    Everyone,
    FollowersOnly,
    OnlyMe,
    Nobody
}
