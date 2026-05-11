using System;
using System.Threading.Tasks;
using SharedModels;
using SharedModels.Interfaces;

namespace BusinessLogic.NotificationSenders
{

  public class SmsNotificationSender : INotificationSender
  {
    public Task SendNotificationAsync(User user, string message)
    {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"\n[SMS DISPATCHED] To: {user.PhoneNumber} | MSG: {message}");
      Console.ResetColor();
      return Task.CompletedTask;
    }
  }
}