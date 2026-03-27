namespace samvaad_backend.Models.Entities;

public enum SocietyRole { Member, Admin }

public class SocietyMember
{
    public Guid SocietyId { get; set; }
    public Guid UserId { get; set; }
    public SocietyRole Role { get; set; } = SocietyRole.Member;
    public DateTime JoinedAt { get; set; }

    // Navigation properties
    public Society Society { get; set; } = null!;
    public User User { get; set; } = null!;
}
