namespace DataAccessLayer.Entities;

/// table to store fine amounts by type.
/// Adjust fine rates without code changes.
public class FineConfig
{
    public int FineConfigId { get; set; }

    /// Fine type: "LateReturn", "MinorDamage", "DamagedBeyondUsable", "Lost"
    public string FineType { get; set; } = string.Empty;

    /// For "LateReturn" this is per-day (₹10/day).
    /// For damage/lost types, a fix amount
    public decimal Amount { get; set; }

    /// The maximum unpaid fine threshold before borrowing is blocked.
    /// For the "LateReturn" (₹500 by default).
    public decimal? MaxUnpaidFineThreshold { get; set; }
}
