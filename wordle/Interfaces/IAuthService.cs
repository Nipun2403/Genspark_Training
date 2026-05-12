using wordle.Models;

namespace wordle.Interfaces
{
  public interface IAuthService
  {
    User LoginOrRegister(string username, string password);
  }
}