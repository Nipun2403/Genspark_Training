using System;
using SimpleNotificationSystem.Models;
using SimpleNotificationSystem.Interface;

namespace SimpleNotificationSystem.Services
{
  public class EmailNotification : INotificationSender
  {
    public void SendNotification(User user, Notification notification)
    {

      Console.WriteLine($"Email sent to {user.Email} with message: {notification.Message} at {notification.SentAt}");
    }
  }
}
