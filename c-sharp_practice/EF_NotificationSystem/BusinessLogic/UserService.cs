using System.Collections.Generic;
using System.Threading.Tasks;
using SharedModels;
using SharedModels.Exceptions;
using SharedModels.Interfaces;
using BusinessLogic.Validators;

namespace BusinessLogic
{
  public class UserService
  {
    private readonly IUserRepository _userRepository;
    private readonly UserValidator _validator;

    public UserService(IUserRepository userRepository, UserValidator validator)
    {
      _userRepository = userRepository;
      _validator = validator;
    }

    public async Task<User> CreateUserAsync(string name, string email, string phone)
    {
      _validator.ValidateName(name);
      _validator.ValidateEmail(email);
      _validator.ValidatePhone(phone);
      _validator.ValidateContactMethods(email, phone);

      var newUser = new User { Name = name, Email = email, PhoneNumber = phone };
      return await _userRepository.AddUserAsync(newUser);
    }

    public async Task<List<User>> GetAllUsersAsync() => await _userRepository.GetAllUsersAsync();

    public async Task<User> GetUserByIdAsync(int id)
    {
      var user = await _userRepository.GetUserByIdAsync(id);
      if (user == null) throw new NotFoundException($"User with ID {id} not found.");
      return user;
    }

    public async Task<bool> UpdateUserAsync(int id, string name, string email, string phone)
    {
      var user = await GetUserByIdAsync(id);

      if (!string.IsNullOrWhiteSpace(name)) _validator.ValidateName(name);
      if (!string.IsNullOrWhiteSpace(email)) _validator.ValidateEmail(email);
      if (!string.IsNullOrWhiteSpace(phone)) _validator.ValidatePhone(phone);

      string finalEmail = string.IsNullOrWhiteSpace(email) ? user.Email : email;
      string finalPhone = string.IsNullOrWhiteSpace(phone) ? user.PhoneNumber : phone;
      _validator.ValidateContactMethods(finalEmail, finalPhone);

      if (!string.IsNullOrWhiteSpace(name)) user.Name = name;
      if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
      if (!string.IsNullOrWhiteSpace(phone)) user.PhoneNumber = phone;

      return await _userRepository.UpdateUserAsync(user);
    }

    public async Task<bool> DeleteUserAsync(int id) => await _userRepository.DeleteUserAsync(id);
  }
}