namespace BusinessLogicLayer.Models;

/// Data Transfer Object representing a member with unpaid fines.
public class MemberPendingFineDto
{
    public int MemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalUnpaid { get; set; }
}

/// Data Transfer Object representing a popular book.
public class MostBorrowedBookDto
{
    public string ISBN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
}

/// Data Transfer Object representing book copy counts in a category.

public class AvailableByCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int AvailableCopies { get; set; }
}
