using SimpleNotificationSystem.Models;

namespace SimpleNotificationSystem.Interface
{
  public interface INotificationSender
  {
    void SendNotification(User user, Notification notification);
  }
}