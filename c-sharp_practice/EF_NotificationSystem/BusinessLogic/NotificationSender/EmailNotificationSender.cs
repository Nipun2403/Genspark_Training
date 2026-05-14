using System;
using System.Threading.Tasks;
using SharedModels;
using SharedModels.Interfaces;

namespace BusinessLogic.NotificationSenders
{
  public class EmailNotificationSender : INotificationSender
  {
    public Task SendNotificationAsync(User user, string message)
    {
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"\n[EMAIL DISPATCHED] To: {user.Email} | BODY: {message}");
      Console.ResetColor();
      return Task.CompletedTask;
    }
  }
}