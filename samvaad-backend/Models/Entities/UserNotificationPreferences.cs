namespace samvaad_backend.Models.Entities;

public class UserNotificationPreferences
{
    public Guid UserId { get; set; }

    // Push notifications
    public bool PushNewFollower { get; set; } = true;
    public bool PushLikes { get; set; } = true;
    public bool PushReplies { get; set; } = true;
    public bool PushMentions { get; set; } = true;
    public bool PushReposts { get; set; }
    public bool PushDirectMessages { get; set; } = true;

    // Email notifications
    public bool EmailWeeklyDigest { get; set; } = true;
    public bool EmailSecurityAlerts { get; set; } = true;
    public bool EmailProductUpdates { get; set; }

    // Quiet hours
    public bool QuietHoursEnabled { get; set; }
    public TimeOnly? QuietHoursFrom { get; set; }
    public TimeOnly? QuietHoursUntil { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
