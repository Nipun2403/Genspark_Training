using BusinessLogicLayer.Services;
using PresentationLayer.UI;
using System.Linq;

namespace PresentationLayer.Menus;


/// Console menu for returning a book, fully list-driven.

public class ReturnMenu
{
    private readonly ReturnService _returnService;
    private readonly BorrowingService _borrowingService;

    public ReturnMenu(ReturnService returnService, BorrowingService borrowingService)
    {
        _returnService = returnService;
        _borrowingService = borrowingService;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- RETURN A BOOK ---");

        // 1. Fetch all system-wide active borrowings
        var allBorrowings = await _borrowingService.GetAllActiveBorrowingsAsync();
        if (allBorrowings.Count == 0)
        {
            Console.WriteLine("There are no active borrowings in the library.");
            return;
        }

        // 2. Extract distinct members who currently hold books
        var activeMembers = allBorrowings
            .Select(b => b.Member)
            .GroupBy(m => m.MemberId)
            .Select(g => g.First())
            .ToList();

        // 3. User selects member
        var member = InputValidator.GetSelection(
            activeMembers, 
            "Select member returning a book: ", 
            m => $"{m.FullName} ({m.Email})"
        );
        
        if (member == null) return;

        // 4. Get active borrowings specifically for this member
        var memberBorrowings = allBorrowings.Where(b => b.MemberId == member.MemberId).ToList();

        // 5. User selects the exact borrowing to return
        var borrowing = InputValidator.GetSelection(
            memberBorrowings,
            "Select the book to return: ",
            b => $"{b.BookCopy.Book.Title,-25} | Copy #{b.CopyId,-3} | Due: {b.DueDate:yyyy-MM-dd} {(b.DueDate < DateTime.UtcNow ? "[OVERDUE]" : "")}"
        );
        
        if (borrowing == null) return;

        Console.WriteLine($"\nSelected Book: '{borrowing.BookCopy.Book.Title}' (Copy #{borrowing.CopyId})");
        
        // 6. User selects book condition
        Console.WriteLine("\nCondition of the book on return:");
        var conditions = new List<string> { "NoDamage", "MinorDamage", "DamagedBeyondUsable", "Lost" };
        
        var condition = InputValidator.GetSelection(
            conditions,
            "Select condition: ",
            c => c
        );
        
        if (condition == null) return;

        var result = await _returnService.ReturnBookAsync(borrowing.BorrowingId, condition);
        Console.WriteLine(result);
    }
}
