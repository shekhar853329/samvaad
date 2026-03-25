using Microsoft.EntityFrameworkCore;
using samvaad_backend.Models.Entities;
using samvaad_backend.Models.Enums;

namespace samvaad_backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<HashTag> HashTags => Set<HashTag>();
    public DbSet<PostHashTag> PostHashTags => Set<PostHashTag>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Follow> Follow => Set<Follow>();
    public DbSet<Mention> Mentions => Set<Mention>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<StoryView> StoryViews => Set<StoryView>();
    public DbSet<UserNotificationPreferences> UserNotificationPreferences => Set<UserNotificationPreferences>();
    public DbSet<UserPrivacySettings> UserPrivacySettings => Set<UserPrivacySettings>();
    public DbSet<UserTag> UserTags => Set<UserTag>();
    public DbSet<UserFeedPreferences> UserFeedPreferences => Set<UserFeedPreferences>();
    public DbSet<MutedWord> MutedWords => Set<MutedWord>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<Friendship> Friendships => Set<Friendship>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Username).HasMaxLength(50).IsRequired();
            b.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
            b.Property(u => u.Email).HasMaxLength(254).IsRequired();
            b.Property(u => u.AvatarUrl).HasMaxLength(500);
            b.Property(u => u.CoverImageUrl).HasMaxLength(500);
            b.Property(u => u.Bio).HasMaxLength(300);
            b.Property(u => u.Location).HasMaxLength(100);
            b.Property(u => u.Website).HasMaxLength(200);

            b.HasIndex(u => u.Username).IsUnique();
            b.HasIndex(u => u.Email).IsUnique();
            b.HasIndex(u => u.LastSeenAt);
            b.HasIndex(u => u.PinnedPostId);

            b.HasOne(u => u.PinnedPost)
             .WithMany()
             .HasForeignKey(u => u.PinnedPostId)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(u => u.NotificationPreferences)
             .WithOne(np => np.User)
             .HasForeignKey<UserNotificationPreferences>(np => np.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(u => u.PrivacySettings)
             .WithOne(ps => ps.User)
             .HasForeignKey<UserPrivacySettings>(ps => ps.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(u => u.FeedPreferences)
             .WithOne(fp => fp.User)
             .HasForeignKey<UserFeedPreferences>(fp => fp.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Post ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<Post>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Content).HasMaxLength(500).IsRequired();

            b.HasIndex(p => p.CreatedAt);
            b.HasIndex(p => p.ParentPostId);
            b.HasIndex(p => p.RepostOfId);
            b.HasIndex(p => new { p.AuthorId, p.CreatedAt });

            b.HasOne(p => p.Author)
             .WithMany(u => u.Posts)
             .HasForeignKey(p => p.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.ParentPost)
             .WithMany(p => p.Replies)
             .HasForeignKey(p => p.ParentPostId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.RepostOf)
             .WithMany(p => p.Reposts)
             .HasForeignKey(p => p.RepostOfId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── HashTag ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<HashTag>(b =>
        {
            b.HasKey(h => h.Id);
            b.Property(h => h.Name).HasMaxLength(100).IsRequired();
            b.Property(h => h.Category).HasMaxLength(50);

            b.HasIndex(h => h.Name).IsUnique();
            b.HasIndex(h => h.PostCount);
        });

        // ── PostHashTag ─────────────────────────────────────────────────────────
        modelBuilder.Entity<PostHashTag>(b =>
        {
            b.HasKey(ph => new { ph.PostId, ph.HashTagId });
            b.HasIndex(ph => ph.HashTagId);

            b.HasOne(ph => ph.Post)
             .WithMany(p => p.PostHashTags)
             .HasForeignKey(ph => ph.PostId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ph => ph.HashTag)
             .WithMany(h => h.PostHashTags)
             .HasForeignKey(ph => ph.HashTagId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── PostMedia ───────────────────────────────────────────────────────────
        modelBuilder.Entity<PostMedia>(b =>
        {
            b.HasKey(pm => pm.Id);
            b.Property(pm => pm.Url).HasMaxLength(500).IsRequired();
            b.Property(pm => pm.MediaType).HasConversion<string>().IsRequired();
            b.HasIndex(pm => pm.PostId);

            b.HasOne(pm => pm.Post)
             .WithMany(p => p.Media)
             .HasForeignKey(pm => pm.PostId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Like ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<Like>(b =>
        {
            b.HasKey(l => new { l.UserId, l.PostId });
            b.HasIndex(l => l.PostId);

            b.HasOne(l => l.User)
             .WithMany(u => u.Likes)
             .HasForeignKey(l => l.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(l => l.Post)
             .WithMany(p => p.Likes)
             .HasForeignKey(l => l.PostId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Bookmark ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Bookmark>(b =>
        {
            b.HasKey(bk => new { bk.UserId, bk.PostId });
            b.HasIndex(bk => bk.PostId);
            b.HasIndex(bk => new { bk.UserId, bk.CreatedAt });

            b.HasOne(bk => bk.User)
             .WithMany(u => u.Bookmarks)
             .HasForeignKey(bk => bk.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(bk => bk.Post)
             .WithMany(p => p.Bookmarks)
             .HasForeignKey(bk => bk.PostId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Follow ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Follow>(b =>
        {
            b.HasKey(f => new { f.FollowerId, f.FollowingId });
            b.HasIndex(f => f.FollowerId);
            b.HasIndex(f => f.FollowingId);

            b.HasOne(f => f.Follower)
             .WithMany(u => u.Following)
             .HasForeignKey(f => f.FollowerId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(f => f.Following)
             .WithMany(u => u.Followers)
             .HasForeignKey(f => f.FollowingId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Mention ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Mention>(b =>
        {
            b.HasKey(m => new { m.PostId, m.MentionedUserId });
            b.HasIndex(m => m.MentionedUserId);

            b.HasOne(m => m.Post)
             .WithMany(p => p.Mentions)
             .HasForeignKey(m => m.PostId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(m => m.MentionedUser)
             .WithMany(u => u.Mentions)
             .HasForeignKey(m => m.MentionedUserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Conversation ────────────────────────────────────────────────────────
        modelBuilder.Entity<Conversation>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).HasMaxLength(100);
            b.Property(c => c.Status).HasConversion<string>().IsRequired();

            b.HasIndex(c => c.CreatedByUserId);
            b.HasIndex(c => c.LastMessageAt);

            b.HasOne(c => c.CreatedBy)
             .WithMany()
             .HasForeignKey(c => c.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ConversationParticipant ──────────────────────────────────────────────
        modelBuilder.Entity<ConversationParticipant>(b =>
        {
            b.HasKey(cp => new { cp.ConversationId, cp.UserId });
            b.HasIndex(cp => cp.UserId);

            b.HasOne(cp => cp.Conversation)
             .WithMany(c => c.Participants)
             .HasForeignKey(cp => cp.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(cp => cp.User)
             .WithMany(u => u.ConversationParticipants)
             .HasForeignKey(cp => cp.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Message ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Message>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Content).HasMaxLength(4000).IsRequired();

            b.HasIndex(m => m.SenderId);
            b.HasIndex(m => new { m.ConversationId, m.CreatedAt });

            b.HasOne(m => m.Conversation)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(m => m.Sender)
             .WithMany(u => u.SentMessages)
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Notification ────────────────────────────────────────────────────────
        modelBuilder.Entity<Notification>(b =>
        {
            b.HasKey(n => n.Id);
            b.Property(n => n.Type).HasConversion<string>().IsRequired();
            b.Property(n => n.SystemMessage).HasMaxLength(200);

            b.HasIndex(n => n.ActorId);
            b.HasIndex(n => n.PostId);
            b.HasIndex(n => new { n.RecipientId, n.IsRead, n.CreatedAt });

            b.HasOne(n => n.Recipient)
             .WithMany(u => u.ReceivedNotifications)
             .HasForeignKey(n => n.RecipientId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(n => n.Actor)
             .WithMany(u => u.SentNotifications)
             .HasForeignKey(n => n.ActorId)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(n => n.Post)
             .WithMany(p => p.Notifications)
             .HasForeignKey(n => n.PostId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── RefreshToken ────────────────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(rt => rt.Id);
            b.Property(rt => rt.TokenHash).HasMaxLength(512).IsRequired();

            b.HasIndex(rt => rt.TokenHash).IsUnique();
            b.HasIndex(rt => new { rt.UserId, rt.IsRevoked });

            b.HasOne(rt => rt.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(rt => rt.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Story ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<Story>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.MediaUrl).HasMaxLength(500).IsRequired();
            b.HasIndex(s => new { s.AuthorId, s.ExpiresAt });

            b.HasOne(s => s.Author)
             .WithMany(u => u.Stories)
             .HasForeignKey(s => s.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── StoryView ────────────────────────────────────────────────────────────
        modelBuilder.Entity<StoryView>(b =>
        {
            b.HasKey(sv => new { sv.StoryId, sv.ViewerId });
            b.HasIndex(sv => sv.ViewerId);

            b.HasOne(sv => sv.Story)
             .WithMany(s => s.Views)
             .HasForeignKey(sv => sv.StoryId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(sv => sv.Viewer)
             .WithMany(u => u.StoryViews)
             .HasForeignKey(sv => sv.ViewerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── UserNotificationPreferences ─────────────────────────────────────────
        modelBuilder.Entity<UserNotificationPreferences>(b =>
        {
            b.HasKey(np => np.UserId);
        });

        // ── UserPrivacySettings ──────────────────────────────────────────────────
        modelBuilder.Entity<UserPrivacySettings>(b =>
        {
            b.HasKey(ps => ps.UserId);
            b.Property(ps => ps.AccountVisibility).HasConversion<string>().IsRequired();
            b.Property(ps => ps.WhoCanMessage).HasConversion<string>().IsRequired();
            b.Property(ps => ps.WhoCanSeeFollowers).HasConversion<string>().IsRequired();
        });

        // ── UserTag ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<UserTag>(b =>
        {
            b.HasKey(ut => new { ut.UserId, ut.Name });
            b.Property(ut => ut.Name).HasMaxLength(50);

            b.HasOne(ut => ut.User)
             .WithMany(u => u.Tags)
             .HasForeignKey(ut => ut.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── UserFeedPreferences ──────────────────────────────────────────────────
        modelBuilder.Entity<UserFeedPreferences>(b =>
        {
            b.HasKey(fp => fp.UserId);
            b.Property(fp => fp.FeedOrder).HasConversion<string>().IsRequired();
            b.Property(fp => fp.SensitiveContent).HasConversion<string>().IsRequired();
            b.Property(fp => fp.AutoplayVideos).HasConversion<string>().IsRequired();
        });

        // ── MutedWord ────────────────────────────────────────────────────────────
        modelBuilder.Entity<MutedWord>(b =>
        {
            b.HasKey(mw => new { mw.UserId, mw.Word });
            b.Property(mw => mw.Word).HasMaxLength(100).IsRequired();

            b.HasOne(mw => mw.FeedPreferences)
             .WithMany(fp => fp.MutedWords)
             .HasForeignKey(mw => mw.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── FriendRequest ──────────────────────────────────────────────────────
        modelBuilder.Entity<FriendRequest>(b =>
        {
            b.HasKey(fr => fr.Id);
            b.Property(fr => fr.Status).HasConversion<string>().IsRequired();

            b.HasIndex(fr => fr.SenderId);
            b.HasIndex(fr => fr.ReceiverId);
            b.HasIndex(fr => new { fr.SenderId, fr.ReceiverId }).IsUnique();
            b.HasIndex(fr => new { fr.ReceiverId, fr.Status, fr.CreatedAt });

            b.HasOne(fr => fr.Sender)
             .WithMany(u => u.SentFriendRequests)
             .HasForeignKey(fr => fr.SenderId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(fr => fr.Receiver)
             .WithMany(u => u.ReceivedFriendRequests)
             .HasForeignKey(fr => fr.ReceiverId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Friendship (adjacency list — two rows per pair) ────────────────────
        modelBuilder.Entity<Friendship>(b =>
        {
            b.HasKey(f => new { f.UserId, f.FriendId });

            // Primary lookup: "give me all friends of user X" — single-column, no OR
            b.HasIndex(f => f.UserId);

            b.HasOne(f => f.User)
             .WithMany(u => u.Friendships)
             .HasForeignKey(f => f.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(f => f.Friend)
             .WithMany(u => u.FriendedBy)
             .HasForeignKey(f => f.FriendId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
