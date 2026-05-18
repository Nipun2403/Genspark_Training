using BusinessLogicLayer.Services;
using PresentationLayer.UI;

namespace PresentationLayer.Menus;


/// Console menu for borrowing a book, with dynamic list selections for both member and book.

public class BorrowMenu
{
    private readonly BorrowingService _borrowingService;
    private readonly MemberService _memberService;
    private readonly BookService _bookService;

    public BorrowMenu(BorrowingService borrowingService, MemberService memberService, BookService bookService)
    {
        _borrowingService = borrowingService;
        _memberService = memberService;
        _bookService = bookService;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- BORROW A BOOK ---");

        var members = await _memberService.GetAllMembersAsync();
        var activeMembers = members.Where(m => m.IsActive).ToList();
        
        if (activeMembers.Count == 0)
        {
            Console.WriteLine("No active members available to borrow books.");
            return;
        }

        var member = InputValidator.GetSelection(activeMembers, "Select Member: ", m => $"{m.FullName} ({m.Email})");
        if (member == null) return;

        int memberId = member.MemberId;

        // Show member's active borrowings for context
        var activeBorrowings = await _borrowingService.GetActiveBorrowingsAsync(memberId);
        if (activeBorrowings.Count > 0)
        {
            Console.WriteLine($"\nCurrent active borrowings ({activeBorrowings.Count}):");
            foreach (var b in activeBorrowings)
            {
                Console.WriteLine($"  - {b.BookCopy.Book.Title} (Copy #{b.CopyId}) | Due: {b.DueDate:yyyy-MM-dd}");
            }
        }

        // Fetch all books with at least one available copy (Available or MinorDamage)
        var availableBooks = await _bookService.GetAvailableBooksAsync();
        if (availableBooks.Count == 0)
        {
            Console.WriteLine("\nNo books with available copies are currently in the library.");
            return;
        }

        var selectedBook = InputValidator.GetSelection(
            availableBooks,
            "Select a book to borrow: ",
            b => $"{b.Title,-35} | by {b.Author,-20} [{b.Copies.Count(c => c.Status == "Available" || c.Status == "MinorDamage")} copies avail]"
        );
        if (selectedBook == null) return;

        var firstCopy = selectedBook.Copies.FirstOrDefault(c => c.Status == "Available" || c.Status == "MinorDamage");

        if (firstCopy == null)
        {
            Console.WriteLine($"Error: Unexpectedly, no available copy of '{selectedBook.Title}' was found.");
            return;
        }

        Console.WriteLine($"\nSelected Book: '{selectedBook.Title}'");
        Console.WriteLine($"Automatically assigning the first available Copy #{firstCopy.CopyId}...");

        var result = await _borrowingService.BorrowBookAsync(memberId, firstCopy.CopyId);
        Console.WriteLine(result);
    }
}
