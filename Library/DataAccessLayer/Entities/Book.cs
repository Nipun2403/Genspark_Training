namespace DataAccessLayer.Entities;

/// Represents a book title identified by its ISBN.
/// One Book -> multiple physical BookCopies.

public class Book
{
    /// ISBN as PK 
    public string ISBN { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;

    /// Foreign key to BookCategory.
    public int CategoryId { get; set; }
    public int PublishedYear { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Extra Properties to Hold Data and relation between Book, BookCopies and BookCategory.
    public BookCategory Category { get; set; } = null!;
    public List<BookCopy> Copies { get; set; } = new();
}
