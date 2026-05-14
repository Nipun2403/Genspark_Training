using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using SharedModels.Interfaces;

namespace DataAccess
{
  public class UserRepository : IUserRepository
  {
    private readonly AppDbContext _context;

    // Inject the DbContext instead of a raw connection string
    public UserRepository(AppDbContext context)
    {
      _context = context;
    }

    public async Task<User> AddUserAsync(User user)
    {
      _context.Users.Add(user);
      await _context.SaveChangesAsync(); // Executes INSERT automatically
      return user;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
      // best practice for read-only queries 
      return await _context.Users.AsNoTracking().ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
      return await _context.Users.FindAsync(id);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
      _context.Users.Update(user);
      return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
      var user = await _context.Users.FindAsync(id);
      if (user == null) return false;

      _context.Users.Remove(user);
      return await _context.SaveChangesAsync() > 0;
    }
  }
}