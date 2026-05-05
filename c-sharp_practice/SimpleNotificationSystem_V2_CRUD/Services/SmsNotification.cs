using System;
using SimpleNotificationSystem_V2_CRUD.Models;
using SimpleNotificationSystem_V2_CRUD.Interface;

// Same as last project, No changes made here
namespace SimpleNotificationSystem_V2_CRUD.Services
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