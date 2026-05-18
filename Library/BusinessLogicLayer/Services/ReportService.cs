using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services;


/// Provides reporting queries for the library system.

public class ReportService
{
    private readonly LibraryDbContext _context;

    public ReportService(LibraryDbContext context)
    {
        _context = context;
    }

    
    /// Gets all currently borrowed books (active borrowings).
    
    public async Task<List<Borrowing>> GetCurrentlyBorrowedBooksAsync()
    {
        return await _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.BookCopy).ThenInclude(c => c.Book)
            .Where(b => b.Status == "Active")
            .OrderBy(b => b.DueDate)
            .ToListAsync();
    }

    
    /// Gets all overdue books (active borrowings past due date).
    
    public async Task<List<Borrowing>> GetOverdueBooksAsync()
    {
        return await _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.BookCopy).ThenInclude(c => c.Book)
            .Where(b => b.Status == "Active" && b.DueDate < DateTime.UtcNow)
            .OrderBy(b => b.DueDate)
            .ToListAsync();
    }

    
    /// Gets members with pending (unpaid) fines.
    
    public async Task<List<Models.MemberPendingFineDto>> GetMembersWithPendingFinesAsync()
    {
        return await _context.Fines
            .Include(f => f.Member)
            .Where(f => !f.IsPaid)
            .GroupBy(f => new { f.MemberId, f.Member.FullName, f.Member.Email })
            .Select(g => new Models.MemberPendingFineDto
            {
                MemberId = g.Key.MemberId,
                FullName = g.Key.FullName,
                Email = g.Key.Email,
                TotalUnpaid = g.Sum(f => f.Amount - f.PaidAmount)
            })
            .OrderByDescending(x => x.TotalUnpaid)
            .ToListAsync();
    }

    
    /// Gets the most borrowed books ranked by total borrow count.
    
    public async Task<List<Models.MostBorrowedBookDto>> GetMostBorrowedBooksAsync()
    {
        return await _context.Borrowings
            .Include(b => b.BookCopy).ThenInclude(c => c.Book)
            .GroupBy(b => new { b.BookCopy.ISBN, b.BookCopy.Book.Title, b.BookCopy.Book.Author })
            .Select(g => new Models.MostBorrowedBookDto
            {
                ISBN = g.Key.ISBN,
                Title = g.Key.Title,
                Author = g.Key.Author,
                BorrowCount = g.Count()
            })
            .OrderByDescending(x => x.BorrowCount)
            .Take(10)
            .ToListAsync();
    }

    
    /// Gets available books grouped by category.
    
    public async Task<List<Models.AvailableByCategoryDto>> GetAvailableBooksByCategoryAsync()
    {
        return await _context.BookCopies
            .Include(c => c.Book).ThenInclude(b => b.Category)
            .Where(c => c.Status == "Available" || c.Status == "MinorDamage")
            .GroupBy(c => c.Book.Category.CategoryName)
            .Select(g => new Models.AvailableByCategoryDto
            {
                CategoryName = g.Key,
                AvailableCopies = g.Count()
            })
            .OrderBy(x => x.CategoryName)
            .ToListAsync();
    }

    
    /// Gets the full borrowing history for a specific member.
    
    public async Task<List<Borrowing>> GetMemberBorrowingHistoryAsync(int memberId)
    {
        return await _context.Borrowings
            .Include(b => b.BookCopy).ThenInclude(c => c.Book)
            .Where(b => b.MemberId == memberId)
            .OrderByDescending(b => b.BorrowDate)
            .ToListAsync();
    }
}
