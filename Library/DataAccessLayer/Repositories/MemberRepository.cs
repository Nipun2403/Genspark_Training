using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly LibraryDbContext _context;

    public MemberRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Member>> GetAllAsync()
    {
        return await _context.Members
            .OrderBy(m => m.MemberId)
            .ToListAsync();
    }

    public async Task<Member?> GetByIdAsync(int id)
    {
        return await _context.Members.FindAsync(id);
    }

    public async Task AddAsync(Member member)
    {
        if (string.IsNullOrEmpty(member.MembershipType))
        {
            member.MembershipType = "Basic";
        }
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
    }
}
