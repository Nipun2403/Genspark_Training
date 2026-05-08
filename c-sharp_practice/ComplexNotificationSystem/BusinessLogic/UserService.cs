using System.Collections.Generic;
using SharedModels;
using SharedModels.Exceptions;
using DataAccess;
using BusinessLogic.Validators;

// Actual business logic for creating user, etc functions

namespace BusinessLogic
{
  public class UserService
  {
    private readonly UserRepository _userRepository;
    private readonly UserValidator _validator;
    public UserService(UserRepository userRepository, UserValidator validator)
    {
      _userRepository = userRepository;
      _validator = validator;
    }

    public User CreateUser(string name, string email, string phone)
    {
      // The strong validaition i mentioned in validator class.
      // Promotes laziness and avoids searching for bugs in middle of the night.
      _validator.ValidateName(name);
      _validator.ValidateEmail(email);
      _validator.ValidatePhone(phone);
      _validator.ValidateContactMethods(email, phone);

      var newUser = new User { Name = name, Email = email, PhoneNumber = phone };
      return _userRepository.AddUser(newUser);
    }

    public List<User> GetAllUsers() => _userRepository.GetAllUsers();

    public User GetUserById(int id)
    {
      var user = _userRepository.GetUserById(id);
      if (user == null)
        throw new NotFoundException($"User with ID {id} not found.");
      return user;
    }

    // Similar validatio as create user.
    public bool UpdateUser(int id, string name, string email, string phone)
    {
      var user = GetUserById(id);

      if (!string.IsNullOrWhiteSpace(name)) _validator.ValidateName(name);
      if (!string.IsNullOrWhiteSpace(email)) _validator.ValidateEmail(email);
      if (!string.IsNullOrWhiteSpace(phone)) _validator.ValidatePhone(phone);

      string finalEmail = string.IsNullOrWhiteSpace(email) ? user.Email : email;
      string finalPhone = string.IsNullOrWhiteSpace(phone) ? user.PhoneNumber : phone;
      _validator.ValidateContactMethods(finalEmail, finalPhone);

      if (!string.IsNullOrWhiteSpace(name)) user.Name = name;
      if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
      if (!string.IsNullOrWhiteSpace(phone)) user.PhoneNumber = phone;

      return _userRepository.UpdateUser(user);
    }

    public bool DeleteUser(int id) => _userRepository.DeleteUser(id);
  }
}