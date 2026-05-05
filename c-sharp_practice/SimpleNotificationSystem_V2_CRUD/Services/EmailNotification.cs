using System;
using SimpleNotificationSystem_V2_CRUD.Models;
using SimpleNotificationSystem_V2_CRUD.Interface;

// Same as last project, No changes made here
namespace SimpleNotificationSystem_V2_CRUD.Services
{
  public class EmailNotification : INotificationSender
  {
    public void SendNotification(User user, Notification notification)
    {

      Console.WriteLine($"Email sent to {user.Email} with message: {notification.Message} at {notification.SentAt}");
    }
  }
}
