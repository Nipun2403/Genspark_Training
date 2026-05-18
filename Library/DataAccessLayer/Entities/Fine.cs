namespace DataAccessLayer.Entities;

/// Fine charged to a member for late return, damage, or lost book.
/// Supports partial payments via the FinePayments collection.

public class Fine
{
    public int FineId { get; set; }

    /// FK to Member.  Who owes this fine.
    public int MemberId { get; set; }

    /// Fk to the transaction. Which borrow transactio caused this fine
    public int BorrowingId { get; set; }

    /// "LateReturn", "MinorDamage", "DamagedBeyondUsable", "Lost"
    public string FineType { get; set; } = string.Empty;

    /// Total fine amount.
    public decimal Amount { get; set; }

    /// Amount paid . Fine is gone when PaidAmount >= Total Amount.
    public decimal PaidAmount { get; set; } = 0;

    /// True when PaidAmount >= Amount.
    public bool IsPaid { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Each Fine is related to one member only. 
    // Each fine is realted to one transaction only
    // One fine can have multiple payment. Partial Payment
    public Member Member { get; set; } = null!;
    public Borrowing Borrowing { get; set; } = null!;
    public List<FinePayment> Payments { get; set; } = new();
}
