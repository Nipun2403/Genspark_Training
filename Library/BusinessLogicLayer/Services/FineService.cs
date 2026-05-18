using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services;

/// Fine viewing, payment, and history operations.
public class FineService
{
    private readonly LibraryDbContext _context;

    public FineService(LibraryDbContext context)
    {
        _context = context;
    }

    /// Gets all pending (unpaid) fines
    public async Task<List<Fine>> GetPendingFinesAsync(int memberId)
    {
        return await _context.Fines
            .Include(f => f.Borrowing)
                .ThenInclude(b => b.BookCopy)
                    .ThenInclude(c => c.Book)
            .Where(f => f.MemberId == memberId && !f.IsPaid)
            .OrderBy(f => f.CreatedAt)
            .ToListAsync();
    }


    /// Gets the complete fine history

    public async Task<List<Fine>> GetFineHistoryAsync(int memberId)
    {
        return await _context.Fines
            .Include(f => f.Borrowing)
                .ThenInclude(b => b.BookCopy)
                    .ThenInclude(c => c.Book)
            .Include(f => f.Payments)
            .Where(f => f.MemberId == memberId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    /// Makes a payment against a specific fine. Supports partial payments.

    public async Task<string> PayFineAsync(int fineId, decimal paymentAmount)
    {
        if (paymentAmount <= 0)
            return "Error: Payment amount must be greater than zero.";

        var fine = await _context.Fines.FindAsync(fineId);
        if (fine == null)
            return "Error: Fine not found.";

        if (fine.IsPaid)
            return "This fine has already been fully paid.";

        decimal remainingBalance = fine.Amount - fine.PaidAmount;

        if (paymentAmount > remainingBalance)
            return $"Error: Payment (₹{paymentAmount:F2}) exceeds remaining (₹{remainingBalance:F2}).";

        // Record the payment
        var payment = new FinePayment
        {
            FineId = fineId,
            AmountPaid = paymentAmount
        };
        _context.FinePayments.Add(payment);

        // Update the fine
        fine.PaidAmount += paymentAmount;
        if (fine.PaidAmount >= fine.Amount)
            fine.IsPaid = true;

        await _context.SaveChangesAsync();

        decimal newRemaining = fine.Amount - fine.PaidAmount;
        return $"Payment of ₹{paymentAmount:F2} recorded.\n" +
               $"  Fine #{fineId}: ₹{fine.PaidAmount:F2}/₹{fine.Amount:F2} paid.\n" +
               $"  Remaining: ₹{newRemaining:F2} | {(fine.IsPaid ? "FULLY PAID " : "PARTIAL")}";
    }


    /// Gets total unpaid fine for a member using the PostgreSQL function.

    public async Task<decimal> GetTotalUnpaidFineAsync(int memberId)
    {
        var result = await _context.Database
            .SqlQueryRaw<decimal>("SELECT calculate_member_fine({0})", memberId)
            .ToListAsync();
        return result.FirstOrDefault();
    }
}
