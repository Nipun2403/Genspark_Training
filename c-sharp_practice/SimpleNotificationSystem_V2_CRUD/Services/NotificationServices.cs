using SimpleNotificationSystem_V2_CRUD.Models;
using SimpleNotificationSystem_V2_CRUD.Interface;

// Nothing changed here too, same as last project
namespace SimpleNotificationSystem_V2_CRUD.Services
{
  public class NotificationService
  {
    private readonly INotificationSender _notificationSender;

    public NotificationService(INotificationSender notificationSender)
    {
      _notificationSender = notificationSender;
    }

    //  Method to send notification to a user
    public void NotifyUser(User user, string textMessage)
    {
      Notification notification = new Notification(textMessage, DateTime.Now);
      _notificationSender.SendNotification(user, notification);
    }
  }
}