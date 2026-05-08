using System;
using SharedModels;
using SharedModels.Interfaces;

namespace BusinessLogic.NotificationSenders
{
  //  Takes INotification interface and uses it to send SMS notification
  public class SmsNotificationSender : INotificationSender
  {

    // Used Console colors learned in the wordle project.
    public void SendNotification(User user, Notification notification)
    {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"\n[SMS DISPATCHED] To: {user.PhoneNumber}");
      Console.WriteLine($"[MESSAGE] {notification.Message}");
      Console.WriteLine($"[TIMESTAMP] {notification.SentAt}");
      Console.ResetColor();
    }
  }
}