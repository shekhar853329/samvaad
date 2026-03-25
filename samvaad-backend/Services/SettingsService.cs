using Microsoft.EntityFrameworkCore;
using samvaad_backend.Common;
using samvaad_backend.Data;
using samvaad_backend.Models.DTOs.Settings;
using samvaad_backend.Models.Entities;
using samvaad_backend.Models.Enums;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Services;

public class SettingsService(AppDbContext db) : ISettingsService
{
    // ── Notifications ─────────────────────────────────────────────────────────

    public async Task<NotificationPrefsDto> GetNotificationPrefsAsync(Guid userId)
    {
        var prefs = await GetOrCreateNotificationPrefsAsync(userId);
        return MapNotificationPrefs(prefs);
    }

    public async Task<NotificationPrefsDto> SaveNotificationPrefsAsync(
        Guid userId, SaveNotificationPrefsRequest req)
    {
        var prefs = await GetOrCreateNotificationPrefsAsync(userId);

        prefs.PushNewFollower = req.PushNewFollower;
        prefs.PushLikes = req.PushLikes;
        prefs.PushReplies = req.PushReplies;
        prefs.PushMentions = req.PushMentions;
        prefs.PushReposts = req.PushReposts;
        prefs.PushDirectMessages = req.PushDirectMessages;
        prefs.EmailWeeklyDigest = req.EmailWeeklyDigest;
        prefs.EmailSecurityAlerts = req.EmailSecurityAlerts;
        prefs.EmailProductUpdates = req.EmailProductUpdates;
        prefs.QuietHoursEnabled = req.QuietHoursEnabled;
        prefs.QuietHoursFrom = ParseTimeOnly(req.QuietHoursFrom);
        prefs.QuietHoursUntil = ParseTimeOnly(req.QuietHoursUntil);

        await db.SaveChangesAsync();
        return MapNotificationPrefs(prefs);
    }

    private async Task<UserNotificationPreferences> GetOrCreateNotificationPrefsAsync(Guid userId)
    {
        var prefs = await db.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        if (prefs is not null) return prefs;

        prefs = new UserNotificationPreferences { UserId = userId };
        db.UserNotificationPreferences.Add(prefs);
        await db.SaveChangesAsync();
        return prefs;
    }

    private static NotificationPrefsDto MapNotificationPrefs(UserNotificationPreferences p) =>
        new(p.PushNewFollower, p.PushLikes, p.PushReplies, p.PushMentions,
            p.PushReposts, p.PushDirectMessages, p.EmailWeeklyDigest,
            p.EmailSecurityAlerts, p.EmailProductUpdates, p.QuietHoursEnabled,
            p.QuietHoursFrom?.ToString("HH:mm"), p.QuietHoursUntil?.ToString("HH:mm"));

    // ── Privacy ───────────────────────────────────────────────────────────────

    public async Task<PrivacySettingsDto> GetPrivacySettingsAsync(Guid userId)
    {
        var settings = await GetOrCreatePrivacySettingsAsync(userId);
        return MapPrivacySettings(settings);
    }

    public async Task<PrivacySettingsDto> SavePrivacySettingsAsync(
        Guid userId, SavePrivacySettingsRequest req)
    {
        var settings = await GetOrCreatePrivacySettingsAsync(userId);

        if (Enum.TryParse<AccountVisibility>(req.AccountVisibility, out var vis))
            settings.AccountVisibility = vis;
        if (Enum.TryParse<InteractionScope>(req.WhoCanMessage, out var wcm))
            settings.WhoCanMessage = wcm;
        if (Enum.TryParse<InteractionScope>(req.WhoCanSeeFollowers, out var wcsf))
            settings.WhoCanSeeFollowers = wcsf;

        settings.AllowTagging = req.AllowTagging;
        settings.AllowReposts = req.AllowReposts;
        settings.ShowInSearch = req.ShowInSearch;
        settings.ShowActivityStatus = req.ShowActivityStatus;
        settings.PersonalisedRecommendations = req.PersonalisedRecommendations;
        settings.ShareDataWithPartners = req.ShareDataWithPartners;

        // Also update user.IsPrivate to stay in sync
        await db.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsPrivate,
                settings.AccountVisibility == AccountVisibility.Private));

        await db.SaveChangesAsync();
        return MapPrivacySettings(settings);
    }

    private async Task<UserPrivacySettings> GetOrCreatePrivacySettingsAsync(Guid userId)
    {
        var settings = await db.UserPrivacySettings.FirstOrDefaultAsync(p => p.UserId == userId);
        if (settings is not null) return settings;

        settings = new UserPrivacySettings { UserId = userId };
        db.UserPrivacySettings.Add(settings);
        await db.SaveChangesAsync();
        return settings;
    }

    private static PrivacySettingsDto MapPrivacySettings(UserPrivacySettings s) =>
        new(s.AccountVisibility.ToString(), s.WhoCanMessage.ToString(),
            s.WhoCanSeeFollowers.ToString(), s.AllowTagging, s.AllowReposts,
            s.ShowInSearch, s.ShowActivityStatus, s.PersonalisedRecommendations,
            s.ShareDataWithPartners);

    // ── Feed ──────────────────────────────────────────────────────────────────

    public async Task<FeedPrefsDto> GetFeedPrefsAsync(Guid userId)
    {
        var prefs = await GetOrCreateFeedPrefsAsync(userId);
        return MapFeedPrefs(prefs);
    }

    public async Task<FeedPrefsDto> SaveFeedPrefsAsync(Guid userId, SaveFeedPrefsRequest req)
    {
        var prefs = await GetOrCreateFeedPrefsAsync(userId);

        if (Enum.TryParse<FeedOrder>(req.FeedOrder, out var fo)) prefs.FeedOrder = fo;
        if (Enum.TryParse<SensitiveContentPolicy>(req.SensitiveContent, out var sc)) prefs.SensitiveContent = sc;
        if (Enum.TryParse<AutoplayVideos>(req.AutoplayVideos, out var av)) prefs.AutoplayVideos = av;

        prefs.ShowSuggestedPosts = req.ShowSuggestedPosts;
        prefs.ShowTrendingTopics = req.ShowTrendingTopics;
        prefs.ShowLikedPostsInOtherFeeds = req.ShowLikedPostsInOtherFeeds;

        await db.SaveChangesAsync();
        return MapFeedPrefs(prefs);
    }

    public async Task<FeedPrefsDto> AddMutedWordAsync(Guid userId, string word)
    {
        var prefs = await GetOrCreateFeedPrefsAsync(userId);
        var normalized = word.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalized) || normalized.Length > 100)
            throw new AppException("Invalid word.", 400);

        var exists = await db.MutedWords.AnyAsync(mw => mw.UserId == userId && mw.Word == normalized);
        if (!exists)
        {
            db.MutedWords.Add(new MutedWord
            {
                UserId = userId,
                Word = normalized,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        return MapFeedPrefs(prefs);
    }

    public async Task<FeedPrefsDto> RemoveMutedWordAsync(Guid userId, string word)
    {
        var prefs = await GetOrCreateFeedPrefsAsync(userId);
        var normalized = word.Trim().ToLowerInvariant();
        var muted = await db.MutedWords.FirstOrDefaultAsync(mw => mw.UserId == userId && mw.Word == normalized);
        if (muted is not null)
        {
            db.MutedWords.Remove(muted);
            await db.SaveChangesAsync();
        }
        return MapFeedPrefs(prefs);
    }

    private async Task<UserFeedPreferences> GetOrCreateFeedPrefsAsync(Guid userId)
    {
        var prefs = await db.UserFeedPreferences
            .Include(p => p.MutedWords)
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (prefs is not null) return prefs;

        prefs = new UserFeedPreferences { UserId = userId };
        db.UserFeedPreferences.Add(prefs);
        await db.SaveChangesAsync();
        return prefs;
    }

    private static FeedPrefsDto MapFeedPrefs(UserFeedPreferences p) =>
        new(p.FeedOrder.ToString(), p.ShowSuggestedPosts, p.ShowTrendingTopics,
            p.SensitiveContent.ToString(), p.AutoplayVideos.ToString(),
            p.ShowLikedPostsInOtherFeeds,
            (p.MutedWords ?? (ICollection<MutedWord>)[]).Select(mw => mw.Word).ToList());

    // ── Sessions ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SessionDto>> GetSessionsAsync(Guid userId)
    {
        var tokens = await db.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();

        // Mark the most recently created token as "current" (best heuristic without storing session ID in JWT)
        return tokens.Select((rt, idx) => new SessionDto(rt.Id, rt.CreatedAt, rt.ExpiresAt, idx == 0))
                     .ToList();
    }

    public async Task RevokeSessionAsync(Guid userId, Guid sessionId)
    {
        var token = await db.RefreshTokens.FirstOrDefaultAsync(rt =>
            rt.Id == sessionId && rt.UserId == userId && !rt.IsRevoked)
            ?? throw new AppException("Session not found.", 404);

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task RevokeAllOtherSessionsAsync(Guid userId)
    {
        // Keep the most recently created token (current session), revoke the rest
        var tokens = await db.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();

        var toRevoke = tokens.Skip(1).ToList(); // skip the newest (current)
        foreach (var t in toRevoke)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    // ── Account actions ───────────────────────────────────────────────────────

    public async Task UpdateEmailAsync(Guid userId, UpdateEmailRequest request)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new AppException("User not found.", 404);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new AppException("Current password is incorrect.", 400);

        var newEmail = request.NewEmail.Trim().ToLowerInvariant();
        var taken = await db.Users.AnyAsync(u => u.Email == newEmail && u.Id != userId);
        if (taken)
            throw new AppException("An account with this email already exists.", 409);

        user.Email = newEmail;
        await db.SaveChangesAsync();
    }

    public async Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new AppException("User not found.", 404);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new AppException("Current password is incorrect.", 400);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(Guid userId, string password)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new AppException("User not found.", 404);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new AppException("Password is incorrect.", 400);

        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TimeOnly? ParseTimeOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return TimeOnly.TryParseExact(value, "HH:mm", out var t) ? t : null;
    }
}
