using System;
using SimpleNotificationSystem.Models;
using SimpleNotificationSystem.Interface;

namespace SimpleNotificationSystem.Services
{
  public class SmsNotification : INotificationSender
  {
    public void SendNotification(User user, Notification notification)
    {
      // Console Log of the sample SMS notification
      Console.WriteLine($"SMS sent to {user.PhoneNumber} with message: {notification.Message} at {notification.SentAt}");
    }
  }
}