using BusinessLogicLayer.Services;
using BusinessLogicLayer.Models;
using PresentationLayer.UI;

namespace PresentationLayer.Menus;


/// Console menu for viewing reports.

public class ReportMenu
{
    private readonly ReportService _reportService;
    private readonly MemberService _memberService;

    public ReportMenu(ReportService reportService, MemberService memberService)
    {
        _reportService = reportService;
        _memberService = memberService;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("--- REPORTS ---");
            Console.WriteLine("1. Books Currently Borrowed");
            Console.WriteLine("2. Overdue Books");
            Console.WriteLine("3. Members with Pending Fines");
            Console.WriteLine("4. Most Borrowed Books");
            Console.WriteLine("5. Available Books by Category");
            Console.WriteLine("6. Member Borrowing History");
            Console.WriteLine("0. Back to Main Menu");
            
            var choice = InputValidator.GetString("Select: ");
            switch (choice)
            {
                case "1": await ShowCurrentlyBorrowedAsync(); break;
                case "2": await ShowOverdueBooksAsync(); break;
                case "3": await ShowMembersWithFinesAsync(); break;
                case "4": await ShowMostBorrowedAsync(); break;
                case "5": await ShowAvailableByCategoryAsync(); break;
                case "6": await ShowMemberHistoryAsync(); break;
                case "0": back = true; break;
                default: Console.WriteLine("  [Error] Invalid option."); break;
            }
        }
    }

    private async Task ShowCurrentlyBorrowedAsync()
    {
        var borrowings = await _reportService.GetCurrentlyBorrowedBooksAsync();
        if (borrowings.Count == 0)
        {
            Console.WriteLine("No books are currently borrowed.");
            return;
        }

        Console.WriteLine($"\n{"Member",-25} {"Book",-30} {"Copy#",-8} {"Borrow Date",-14} {"Due Date",-14}");
        Console.WriteLine(new string('-', 93));
        foreach (var b in borrowings)
        {
            Console.WriteLine($"{b.Member.FullName,-25} {b.BookCopy.Book.Title,-30} {b.CopyId,-8} {b.BorrowDate:yyyy-MM-dd}    {b.DueDate:yyyy-MM-dd}");
        }
    }

    private async Task ShowOverdueBooksAsync()
    {
        var borrowings = await _reportService.GetOverdueBooksAsync();
        if (borrowings.Count == 0)
        {
            Console.WriteLine("No overdue books.");
            return;
        }

        Console.WriteLine($"\n{"Member",-25} {"Book",-30} {"Due Date",-14} {"Days Overdue",-12}");
        Console.WriteLine(new string('-', 83));
        foreach (var b in borrowings)
        {
            int daysOverdue = (int)(DateTime.UtcNow - b.DueDate).TotalDays;
            Console.WriteLine($"{b.Member.FullName,-25} {b.BookCopy.Book.Title,-30} {b.DueDate:yyyy-MM-dd}    {daysOverdue,-12}");
        }
    }

    private async Task ShowMembersWithFinesAsync()
    {
        var members = await _reportService.GetMembersWithPendingFinesAsync();
        if (members.Count == 0)
        {
            Console.WriteLine("No members with pending fines.");
            return;
        }

        Console.WriteLine($"\n{"ID",-6} {"Name",-25} {"Email",-30} {"Unpaid",-10}");
        Console.WriteLine(new string('-', 73));
        foreach (var m in members)
        {
            Console.WriteLine($"{m.MemberId,-6} {m.FullName,-25} {m.Email,-30} ₹{m.TotalUnpaid,-9:F2}");
        }
    }

    private async Task ShowMostBorrowedAsync()
    {
        var books = await _reportService.GetMostBorrowedBooksAsync();
        if (books.Count == 0)
        {
            Console.WriteLine("No borrowing data available.");
            return;
        }

        Console.WriteLine($"\n{"#",-4} {"ISBN",-18} {"Title",-30} {"Author",-20} {"Count",-6}");
        Console.WriteLine(new string('-', 80));
        int rank = 1;
        foreach (var b in books)
        {
            Console.WriteLine($"{rank++,-4} {b.ISBN,-18} {b.Title,-30} {b.Author,-20} {b.BorrowCount,-6}");
        }
    }

    private async Task ShowAvailableByCategoryAsync()
    {
        var categories = await _reportService.GetAvailableBooksByCategoryAsync();
        if (categories.Count == 0)
        {
            Console.WriteLine("No available books.");
            return;
        }

        Console.WriteLine($"\n{"Category",-25} {"Available Copies",-15}");
        Console.WriteLine(new string('-', 42));
        foreach (var c in categories)
        {
            Console.WriteLine($"{c.CategoryName,-25} {c.AvailableCopies,-15}");
        }
    }

    private async Task ShowMemberHistoryAsync()
    {
        var members = await _memberService.GetAllMembersAsync();
        if (members.Count == 0)
        {
            Console.WriteLine("No members found.");
            return;
        }

        var member = InputValidator.GetSelection(members, "Select Member to view history: ", m => $"{m.FullName} ({m.Email})");
        if (member == null) return;

        var borrowings = await _reportService.GetMemberBorrowingHistoryAsync(member.MemberId);
        if (borrowings.Count == 0)
        {
            Console.WriteLine("No borrowing history for this member.");
            return;
        }

        Console.WriteLine($"\n{"ID",-6} {"Book",-30} {"Borrow Date",-14} {"Due Date",-14} {"Return Date",-14} {"Status",-10}");
        Console.WriteLine(new string('-', 95));
        foreach (var b in borrowings)
        {
            string returnDateStr = b.ReturnDate?.ToString("yyyy-MM-dd") ?? "Not Returned";
            Console.WriteLine($"{b.BorrowingId,-6} {b.BookCopy.Book.Title,-30} {b.BorrowDate:yyyy-MM-dd}    {b.DueDate:yyyy-MM-dd}    {returnDateStr,-14} {b.Status,-10}");
        }
    }
}
