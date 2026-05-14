using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedModels.Interfaces
{
  public interface IUserRepository
  {
    Task<User> AddUserAsync(User user);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int id);
  }
}