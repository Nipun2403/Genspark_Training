namespace DataAccessLayer.Entities;

/// A single borrowing transaction linking a Member to a BookCopy.
/// borrow/return dates, due date, and the condition of the copy

public class Borrowing
{
    public int BorrowingId { get; set; }

    /// FK to member table. Who borrowed the book.
    public int MemberId { get; set; }

    /// FK to BookCopy. Which copy was borrowed.
    public int CopyId { get; set; }

    public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

    /// Calculated as BorrowDate + MaxBorrowDays from MembershipConfig.
    public DateTime DueDate { get; set; }

    /// Null while the book is borrowed. Set when the book is returned.
    public DateTime? ReturnDate { get; set; }

    /// "Active" while borrowed, "Returned" when return.
    public string Status { get; set; } = "Active";

    /// Records the condition of the copy at the time of borrowing.
    /// Fine is calculated on the difference 
    /// "Available", "MinorDamage"
    public string ConditionAtBorrow { get; set; } = string.Empty;

    /// Return Condition of the copy. Calculated when returned
    /// "NoDamage", "MinorDamage", "DamagedBeyondUsable", "Lost"
    public string? ConditionAtReturn { get; set; }

    // Each transaction will be associatied with one member and one book copy.
    // But transaction can have multiple fine. One for late, one for damage, etc.
    public Member Member { get; set; } = null!;
    public BookCopy BookCopy { get; set; } = null!;
    public List<Fine> Fines { get; set; } = new();
}
