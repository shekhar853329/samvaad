using samvaad_backend.Models.DTOs.Settings;

namespace samvaad_backend.Services.Interfaces;

public interface ISettingsService
{
    // Notification preferences
    Task<NotificationPrefsDto> GetNotificationPrefsAsync(Guid userId);
    Task<NotificationPrefsDto> SaveNotificationPrefsAsync(Guid userId, SaveNotificationPrefsRequest request);

    // Privacy settings
    Task<PrivacySettingsDto> GetPrivacySettingsAsync(Guid userId);
    Task<PrivacySettingsDto> SavePrivacySettingsAsync(Guid userId, SavePrivacySettingsRequest request);

    // Feed preferences
    Task<FeedPrefsDto> GetFeedPrefsAsync(Guid userId);
    Task<FeedPrefsDto> SaveFeedPrefsAsync(Guid userId, SaveFeedPrefsRequest request);
    Task<FeedPrefsDto> AddMutedWordAsync(Guid userId, string word);
    Task<FeedPrefsDto> RemoveMutedWordAsync(Guid userId, string word);

    // Sessions
    Task<IReadOnlyList<SessionDto>> GetSessionsAsync(Guid userId);
    Task RevokeSessionAsync(Guid userId, Guid sessionId);
    Task RevokeAllOtherSessionsAsync(Guid userId);

    // Account actions
    Task UpdateEmailAsync(Guid userId, UpdateEmailRequest request);
    Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request);
    Task DeleteAccountAsync(Guid userId, string password);
}
