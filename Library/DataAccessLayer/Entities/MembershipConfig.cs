namespace DataAccessLayer.Entities;

/// Configuration table storing detailes about membership and limits
/// Easy to change on the fly.
/// Better future proofing ?

public class MembershipConfig
{
    public int ConfigId { get; set; }

    /// "Basic", "Student", or "Premium".

    public string MembershipType { get; set; } = string.Empty;


    /// Max books a member can have at once

    public int MaxActiveBorrowings { get; set; }


    /// Max number of days

    public int MaxBorrowDays { get; set; }

    // Each membership config will have multiple user to it. One to many
    public List<Member> Members { get; set; } = new();
}
