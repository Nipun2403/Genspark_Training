using SimpleNotificationSystem.Models;
using SimpleNotificationSystem.Interface;

namespace SimpleNotificationSystem.Services
{
  public class NotificationService
  {
    private readonly INotificationSender _notificationSender;

    public NotificationService(INotificationSender notificationSender)
    {
      _notificationSender = notificationSender;
    }

    public void NotifyUser(User user, string textMessage)
    {
      Notification notification = new Notification(textMessage);
      _notificationSender.SendNotification(user, notification);
    }
  }
}