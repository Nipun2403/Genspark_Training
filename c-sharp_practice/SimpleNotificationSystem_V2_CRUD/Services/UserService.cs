using System;
using System.Collections.Generic;
using System.Linq;
using SimpleNotificationSystem_V2_CRUD.Models;

namespace SimpleNotificationSystem_V2_CRUD.Services
{
  public class UserService
  {
    private List<User> _users = new List<User>();

    private int _nextId = 1;

    // Create new User
    public User CreateUser(string name, string email, string phoneNumber)
    {
      User newUser = new User
      {
        Id = _nextId++,
        Name = name,
        Email = email,
        PhoneNumber = phoneNumber
      };
      _users.Add(newUser);
      return newUser;
    }

    // Read all Users

    public List<User> GetAllUsers()
    {
      return _users;
    }

    // Read User by ID
    public User? GetUserById(int id)
    {
      return _users.FirstOrDefault(u => u.Id == id);
    }

    // Update User
    public bool UpdateUser(int id, string newName, string newEmail, string newPhoneNumber)
    {
      User? user = GetUserById(id);
      if (user == null) return false;

      if (!string.IsNullOrEmpty(newName)) user.Name = newName;
      if (!string.IsNullOrEmpty(newEmail)) user.Email = newEmail;
      if (!string.IsNullOrEmpty(newPhoneNumber)) user.PhoneNumber = newPhoneNumber;
      return true;
    }

    // Delete User
    public bool DeleteUser(int id)
    {
      User? user = GetUserById(id);
      if (user == null) return false;

      _users.Remove(user);
      return true;
    }
  }
}