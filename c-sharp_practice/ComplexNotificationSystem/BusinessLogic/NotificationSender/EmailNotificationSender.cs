using System;
using SharedModels;
using SharedModels.Interfaces;

namespace BusinessLogic.NotificationSenders
{
  // Takes INotification interface and impelemtns it to send email notification
  public class EmailNotificationSender : INotificationSender
  {
    public void SendNotification(User user, Notification notification)
    {
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"\n[EMAIL DISPATCHED] To: {user.Email}");
      Console.WriteLine($"[SUBJECT] System Alert");
      Console.WriteLine($"[BODY] {notification.Message}");
      Console.WriteLine($"[TIMESTAMP] {notification.SentAt}");
      Console.ResetColor();
    }
  }
}