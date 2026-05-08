using BusinessLogic;
using BusinessLogic.Validators;
using DataAccess;

namespace PresentationUI
{
  class Program
  {
    static void Main(string[] args)
    {
      // Initialize Repositories
      var userRepository = new UserRepository();
      var notificationRepository = new NotificationRepository();

      // Initialize the validator to be used in both UserService and ConsoleApp to ensure strong validation.
      var userValidator = new UserValidator();

      // Pass user repository and custom validator to user service
      var userService = new UserService(userRepository, userValidator);
      // Passing userservice and notification repos to notification service.
      var notificationService = new NotificationService(notificationRepository, userService);

      // Passing the validator along with others to ensure that any input errors are caught in the Ui layer fast.
      var terminal = new ConsoleApplication(userService, notificationService, userValidator);

      // Let'g Goo, Start the app :)
      terminal.Run();
    }
  }
}