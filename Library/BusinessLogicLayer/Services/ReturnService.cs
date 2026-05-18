using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services;


/// Book return workflow including late fine calculation and damage assessment.

public class ReturnService
{
    private readonly LibraryDbContext _context;

    public ReturnService(LibraryDbContext context)
    {
        _context = context;
    }


    /// Processes a book return 
    /// "NoDamage", "MinorDamage", "DamagedBeyondUsable", "Lost"

    public async Task<string> ReturnBookAsync(int borrowingId, string conditionAtReturn)
    {
        var validConditions = new[] { "NoDamage", "MinorDamage", "DamagedBeyondUsable", "Lost" };
        if (!validConditions.Contains(conditionAtReturn))
            return $"Error: Invalid return condition. Valid values: {string.Join(", ", validConditions)}";

        // Find the active borrowing
        var borrowing = await _context.Borrowings
            .Include(b => b.BookCopy)
                .ThenInclude(c => c.Book)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.BorrowingId == borrowingId && b.Status == "Active");

        if (borrowing == null)
            return "Error: Active borrowing not found.";

        var messages = new List<string>();

        // Mark borrowing as returned 
        borrowing.ReturnDate = DateTime.UtcNow;
        borrowing.Status = "Returned";
        borrowing.ConditionAtReturn = conditionAtReturn;

        //  Calculate late return fine 
        if (borrowing.ReturnDate > borrowing.DueDate)
        {
            int daysLate = (int)(borrowing.ReturnDate.Value - borrowing.DueDate).TotalDays;

            // Get the per-day fine rate from config
            var lateConfig = await _context.FineConfigs
                .FirstOrDefaultAsync(fc => fc.FineType == "LateReturn");

            decimal perDayRate = lateConfig?.Amount ?? 10m;
            decimal lateFineAmount = daysLate * perDayRate;

            var lateFine = new Fine
            {
                MemberId = borrowing.MemberId,
                BorrowingId = borrowingId,
                FineType = "LateReturn",
                Amount = lateFineAmount
            };

            _context.Fines.Add(lateFine);
            messages.Add($"Late return fine: ₹{lateFineAmount:F2} ({daysLate} days × ₹{perDayRate:F2}/day)");
        }

        // Assess damage fine 
        if (conditionAtReturn == "MinorDamage")
        {
            // Only charge damage fine if the copy was in good condition when borrowed
            if (borrowing.ConditionAtBorrow == "Available")
            {
                var damageConfig = await _context.FineConfigs
                    .FirstOrDefaultAsync(fc => fc.FineType == "MinorDamage");

                decimal damageFine = damageConfig?.Amount ?? 200m;

                _context.Fines.Add(new Fine
                {
                    MemberId = borrowing.MemberId,
                    BorrowingId = borrowingId,
                    FineType = "MinorDamage",
                    Amount = damageFine
                });

                messages.Add($"Minor damage fine: ₹{damageFine:F2}");
            }
            else
            {
                messages.Add("No damage fine — the copy was already damaged when borrowed.");
            }

            // Update copy status
            borrowing.BookCopy.Status = "MinorDamage";
        }
        else if (conditionAtReturn == "DamagedBeyondUsable")
        {
            // Charge damage fine if it was in better condition when borrowed
            if (borrowing.ConditionAtBorrow == "Available" || borrowing.ConditionAtBorrow == "MinorDamage")
            {
                var damageConfig = await _context.FineConfigs
                    .FirstOrDefaultAsync(fc => fc.FineType == "DamagedBeyondUsable");

                decimal damageFine = damageConfig?.Amount ?? 500m;

                _context.Fines.Add(new Fine
                {
                    MemberId = borrowing.MemberId,
                    BorrowingId = borrowingId,
                    FineType = "DamagedBeyondUsable",
                    Amount = damageFine
                });

                messages.Add($"Severe damage fine: ₹{damageFine:F2}");
            }

            // Copy is retired from lending
            borrowing.BookCopy.Status = "DamagedBeyondUsable";
        }
        else if (conditionAtReturn == "Lost")
        {
            var lostConfig = await _context.FineConfigs
                .FirstOrDefaultAsync(fc => fc.FineType == "Lost");

            decimal lostFine = lostConfig?.Amount ?? 1000m;

            _context.Fines.Add(new Fine
            {
                MemberId = borrowing.MemberId,
                BorrowingId = borrowingId,
                FineType = "Lost",
                Amount = lostFine
            });

            borrowing.BookCopy.Status = "Lost";
            messages.Add($"Lost book fine: ₹{lostFine:F2}");
        }
        else // NoDamage
        {
            // Restore copy to its original condition (Available or MinorDamage)
            // If it was MinorDamage when borrowed and returned with no further damage, it stays MinorDamage
            borrowing.BookCopy.Status = borrowing.ConditionAtBorrow == "MinorDamage"
                ? "MinorDamage"
                : "Available";
        }

        await _context.SaveChangesAsync();

        string result = $"Book returned successfully!\n" +
                        $"  Book: {borrowing.BookCopy.Book.Title} (Copy #{borrowing.CopyId})\n" +
                        $"  Member: {borrowing.Member.FullName}\n" +
                        $"  Copy status: {borrowing.BookCopy.Status}";

        if (messages.Count > 0)
        {
            result += "\n  Fines:\n    " + string.Join("\n    ", messages);
        }
        else
        {
            result += "\n  No fines.";
        }

        return result;
    }
}
