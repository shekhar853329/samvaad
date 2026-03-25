namespace samvaad_backend.Models.DTOs.Settings;

public record NotificationPrefsDto(
    bool PushNewFollower,
    bool PushLikes,
    bool PushReplies,
    bool PushMentions,
    bool PushReposts,
    bool PushDirectMessages,
    bool EmailWeeklyDigest,
    bool EmailSecurityAlerts,
    bool EmailProductUpdates,
    bool QuietHoursEnabled,
    string? QuietHoursFrom,
    string? QuietHoursUntil
);

public class SaveNotificationPrefsRequest
{
    public bool PushNewFollower { get; set; }
    public bool PushLikes { get; set; }
    public bool PushReplies { get; set; }
    public bool PushMentions { get; set; }
    public bool PushReposts { get; set; }
    public bool PushDirectMessages { get; set; }
    public bool EmailWeeklyDigest { get; set; }
    public bool EmailSecurityAlerts { get; set; }
    public bool EmailProductUpdates { get; set; }
    public bool QuietHoursEnabled { get; set; }
    /// <summary>"HH:mm" 24-hour format e.g. "22:00"</summary>
    public string? QuietHoursFrom { get; set; }
    /// <summary>"HH:mm" 24-hour format e.g. "07:00"</summary>
    public string? QuietHoursUntil { get; set; }
}
