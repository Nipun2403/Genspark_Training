namespace DataAccessLayer.Entities;

/// A single payment made against a Fine.
/// A fine can have multiple partial payments.
public class FinePayment
{
    public int PaymentId { get; set; }

    /// FK to the fine being paid. Which fine is this payment for.
    public int FineId { get; set; }

    /// Amount paid
    public decimal AmountPaid { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    // One Payment can be linked only to one fine
    public Fine Fine { get; set; } = null!;
}
