namespace DataAccessLayer.Entities;

/// Represents a physical copy of a book. Multiple copies can exist for one ISBN.
/// Only copies can be issuesd, not the asbtract book itself

public class BookCopy
{
    public int CopyId { get; set; }

    /// Foreign key to Book (ISBN).
    public string ISBN { get; set; } = string.Empty;


    /// Current status of this copy.
    /// "Available", "Borrowed", "MinorDamage", "DamagedBeyondUsable", "Lost"
    public string Status { get; set; } = "Available";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Book copy belongs to one book only,
    //  But One copy can have multiple borrowings. Not parrallel but like history of borrowings.
    public Book Book { get; set; } = null!;
    public List<Borrowing> Borrowings { get; set; } = new();
}
