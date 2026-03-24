using samvaad_backend.Models.Enums;

namespace samvaad_backend.Models.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public bool IsGroup { get; set; }
    public string? Name { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;
    public DateTime LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public ICollection<ConversationParticipant> Participants { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}
