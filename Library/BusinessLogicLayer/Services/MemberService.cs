using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services;

/// CURD: add, view, search, update, deactivate.
public class MemberService
{
    private readonly LibraryDbContext _context;

    public MemberService(LibraryDbContext context)
    {
        _context = context;
    }


    /// Adds a new member to the library.
    /// Validates that email and phone are unique, and membership type exists.

    public async Task<string> AddMemberAsync(string fullName, string email, string phoneNumber, string membershipType)
    {
        // Check if membership type exists in config (case-insensitively)
        var dbConfig = await _context.MembershipConfigs
            .FirstOrDefaultAsync(mc => mc.MembershipType.ToLower() == membershipType.ToLower());

        if (dbConfig == null)
            return $"Error: Membership type '{membershipType}' does not exist.";

        // Check for duplicate email (case-insensitively)
        var emailExists = await _context.Members.AnyAsync(m => m.Email.ToLower() == email.ToLower());
        if (emailExists)
            return $"Error: A member with email '{email}' already exists.";

        // Check for duplicate phone number
        var phoneExists = await _context.Members.AnyAsync(m => m.PhoneNumber == phoneNumber);
        if (phoneExists)
            return $"Error: A member with phone number '{phoneNumber}' already exists.";

        var member = new Member
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            MembershipType = membershipType
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return $"Member '{fullName}' added successfully with ID: {member.MemberId}";
    }


    /// Returns all members
    public async Task<List<Member>> GetAllMembersAsync()
    {
        return await _context.Members
            .Include(m => m.MembershipConfig)
            .OrderBy(m => m.MemberId)
            .ToListAsync();
    }


    /// Searches by phone number or email.
    public async Task<Member?> SearchMemberAsync(string searchTerm)
    {
        return await _context.Members
            .Include(m => m.MembershipConfig)
            .FirstOrDefaultAsync(m => m.Email.ToLower() == searchTerm.ToLower() || m.PhoneNumber == searchTerm);
    }


    /// Updates the membership

    public async Task<string> UpdateMembershipTypeAsync(int memberId, string newMembershipType)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
            return "Error: Member not found.";

        // Case-insensitive lookup
        var dbConfig = await _context.MembershipConfigs
            .FirstOrDefaultAsync(mc => mc.MembershipType.ToLower() == newMembershipType.ToLower());

        if (dbConfig == null)
            return $"Error: Membership type '{newMembershipType}' does not exist.";

        member.MembershipType = dbConfig.MembershipType;
        await _context.SaveChangesAsync();

        return $"Member {memberId} membership updated to '{dbConfig.MembershipType}'.";
    }

    /// Deactivates a member only if they have no active borrowings.
    public async Task<string> DeactivateMemberAsync(int memberId)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
            return "Error: Member not found.";

        if (!member.IsActive)
            return "Member is already inactive.";

        // Check if there are any active borrowings
        var activeBorrowings = await _context.Borrowings
            .Include(b => b.BookCopy)
                .ThenInclude(c => c.Book)
            .Where(b => b.MemberId == memberId && b.Status == "Active")
            .ToListAsync();

        if (activeBorrowings.Any())
        {
            var bookList = string.Join(", ", activeBorrowings.Select(b => $"'{b.BookCopy.Book.Title}' (Copy #{b.CopyId})"));
            return $"Error: Can't deactivate, currently has book: {bookList}";
        }

        member.IsActive = false;
        await _context.SaveChangesAsync();

        return $"Member {memberId} ({member.FullName}) has been deactivated.";
    }

    /// Reactivates a deactivated member.
    public async Task<string> ReactivateMemberAsync(int memberId)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
            return "Error: Member not found.";

        if (member.IsActive)
            return "Member is already active.";

        member.IsActive = true;
        await _context.SaveChangesAsync();

        return $"Member {memberId} ({member.FullName}) has been successfully reactivated! Welcome back!";
    }
    /// Gets all available membership configurations (types, limits, fees).
    public async Task<List<MembershipConfig>> GetAllMembershipConfigsAsync()
    {
        return await _context.MembershipConfigs
            .OrderBy(m => m.MembershipType)
            .ToListAsync();
    }
}
