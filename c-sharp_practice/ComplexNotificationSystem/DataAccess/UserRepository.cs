using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Principal;
using SharedModels;

// CRUD Operation for User entity. 

namespace DataAccess
{
  public class UserRepository
  {
    private readonly List<User> _users = [];
    private int _nextId = 1;

    public User AddUser(User user)
    {
      user.Id = _nextId++;
      _users.Add(user);
      return user;
    }


    public List<User> GetAllUsers()
    {
      return [.. _users];
    }
    public User? GetUserById(int id) => _users.FirstOrDefault(u => u.Id == id);

    public bool UpdateUser(User user)
    {
      var oldUser = GetUserById(user.Id);

      if (oldUser == null) { return false; }

      oldUser.Name = user.Name;
      oldUser.Email = user.Email;
      oldUser.PhoneNumber = user.PhoneNumber;
      return true;
    }

    public bool DeleteUser(int id)
    {
      var oldUser = GetUserById(id);
      if (oldUser == null) { return false; }

      _users.Remove(oldUser);
      return true;
    }


  }
}