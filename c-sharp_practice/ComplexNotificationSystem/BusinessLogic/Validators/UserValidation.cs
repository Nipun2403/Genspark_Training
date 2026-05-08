using System.Text.RegularExpressions;
using SharedModels.Exceptions;

namespace BusinessLogic.Validators
{
  // Varrious types of validation scenarious to ensure proper data is processsed for UserService. Ensure separation of concerns by removing input checking login frrom the userService file
  // Using strong validation here when creating user or updating user to not have headache later while doing every other steps or managing user data.
  // Also cz I'm lazy to do validation in every other file :(
  public class UserValidator
  {
    public void ValidateName(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
        throw new ValidationException("Name cannot be empty.");

      if (!Regex.IsMatch(name, @"^[a-zA-Z\s\-']+$"))
        throw new ValidationException("Name must contain only letters, spaces, hyphens, or apostrophes.");
    }

    public void ValidateEmail(string email)
    {
      if (string.IsNullOrWhiteSpace(email)) return;

      if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        throw new ValidationException("Invalid email format (e.g., user@domain.com).");
    }

    public void ValidatePhone(string phone)
    {
      if (string.IsNullOrWhiteSpace(phone)) return;

      if (!Regex.IsMatch(phone, @"^[\d\-\+\s\(\)]+$") || phone.Length < 7)
        throw new ValidationException("Invalid phone format. Must be at least 7 digits.");
    }

    // To ensure either one of the detials is provided to again, not have a headace later when sending notification or anything else. 
    public void ValidateContactMethods(string email, string phone)
    {
      if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
        throw new ValidationException("At least one contact method (Email or Phone) is strictly required.");
    }
  }
}