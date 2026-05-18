namespace DataAccessLayer.Entities;

/// A library member

public class Member
{
    public int MemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    /// "Basic", "Student", or "Premium".
    /// Links to MembershipConfig for borrowing limits.
    public string MembershipType { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    // Relation to membersbhip config for borrowing limits and all
    // One member can have multipe borrowings and multiple fines.
    public MembershipConfig MembershipConfig { get; set; } = null!;
    public List<Borrowing> Borrowings { get; set; } = new();
    public List<Fine> Fines { get; set; } = new();
}
