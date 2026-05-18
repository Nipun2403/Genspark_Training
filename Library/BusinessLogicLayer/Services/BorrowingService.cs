using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services;

/// Handles the book borrowing.
/// All 5 validation checks.

public class BorrowingService
{
    private readonly LibraryDbContext _context;

    public BorrowingService(LibraryDbContext context)
    {
        _context = context;
    }

    /// Attempts to borrow a book copy for a member.
    /// Runs all validations inside a transaction. If any check fails, the transaction is rolled back.
    public async Task<string> BorrowBookAsync(int memberId, int copyId)
    {
        // Start a database transaction
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate member exists and is active 
            var member = await _context.Members
                .Include(m => m.MembershipConfig)
                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null)
            {
                await transaction.RollbackAsync();
                return "Error: Member not found.";
            }

            if (!member.IsActive)
            {
                await transaction.RollbackAsync();
                return "Error: Member is inactive. Cannot borrow books.";
            }

            // check unpaid fines ≤ threshold 
            // Get the fine threshold from config
            var fineThresholdConfig = await _context.FineConfigs
                .FirstOrDefaultAsync(fc => fc.MaxUnpaidFineThreshold != null);

            decimal maxUnpaidThreshold = fineThresholdConfig?.MaxUnpaidFineThreshold ?? 500m;

            // Calculate total unpaid fines
            decimal totalUnpaidFines = await _context.Fines
                .Where(f => f.MemberId == memberId && !f.IsPaid)
                .SumAsync(f => f.Amount - f.PaidAmount);

            if (totalUnpaidFines > maxUnpaidThreshold)
            {
                await transaction.RollbackAsync();
                return $"Error: Cannot borrow. Unpaid fines (₹{totalUnpaidFines:F2}) exceed the maximum allowed (₹{maxUnpaidThreshold:F2}).";
            }

            // Check active borrowing count against membership limit
            int activeBorrowings = await _context.Borrowings
                .CountAsync(b => b.MemberId == memberId && b.Status == "Active");

            if (activeBorrowings >= member.MembershipConfig.MaxActiveBorrowings)
            {
                await transaction.RollbackAsync();
                return $"Error: Borrowing limit reached. {member.MembershipType} members can borrow up to {member.MembershipConfig.MaxActiveBorrowings} books.";
            }

            // Check book copy availability
            var bookCopy = await _context.BookCopies
                .Include(c => c.Book)
                .FirstOrDefaultAsync(c => c.CopyId == copyId);

            if (bookCopy == null)
            {
                await transaction.RollbackAsync();
                return "Error: Book copy not found.";
            }

            if (bookCopy.Status != "Available" && bookCopy.Status != "MinorDamage")
            {
                await transaction.RollbackAsync();
                return $"Error: This copy is not available for borrowing. Current status: {bookCopy.Status}.";
            }

            // Check for duplicate active borrowing of the same ISBN
            bool alreadyBorrowed = await _context.Borrowings
                .Include(b => b.BookCopy)
                .AnyAsync(b =>
                    b.MemberId == memberId &&
                    b.Status == "Active" &&
                    b.BookCopy.ISBN == bookCopy.ISBN);

            if (alreadyBorrowed)
            {
                await transaction.RollbackAsync();
                return $"Error: You already have an active borrowing for '{bookCopy.Book.Title}' (ISBN: {bookCopy.ISBN}).";
            }

            // Create borrowing transaction
            var borrowing = new Borrowing
            {
                MemberId = memberId,
                CopyId = copyId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(member.MembershipConfig.MaxBorrowDays),
                ConditionAtBorrow = bookCopy.Status // Record the copy's condition at borrow time
            };

            _context.Borrowings.Add(borrowing);

            // Update book copy status to "Borrowed"
            bookCopy.Status = "Borrowed";

            await _context.SaveChangesAsync();

            //  Complete the transaction 
            await transaction.CommitAsync();

            return $"Book borrowed successfully!\n" +
                   $"  Book: {bookCopy.Book.Title} (Copy #{copyId})\n" +
                   $"  Borrowed by: {member.FullName} (ID: {memberId})\n" +
                   $"  Due date: {borrowing.DueDate:yyyy-MM-dd}";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return $"Error: An unexpected error occurred during borrowing. {ex.Message}";
        }
    }

    /// Gets all active borrowings for a member
    public async Task<List<Borrowing>> GetActiveBorrowingsAsync(int memberId)
    {
        return await _context.Borrowings
            .Include(b => b.BookCopy)
                .ThenInclude(c => c.Book)
            .Where(b => b.MemberId == memberId && b.Status == "Active")
            .OrderBy(b => b.DueDate)
            .ToListAsync();
    }

    /// Gets all active borrowings in the entire system.
    public async Task<List<Borrowing>> GetAllActiveBorrowingsAsync()
    {
        return await _context.Borrowings
            .Include(b => b.Member)
            .Include(b => b.BookCopy).ThenInclude(c => c.Book)
            .Where(b => b.Status == "Active")
            .OrderBy(b => b.DueDate)
            .ToListAsync();
    }
}
