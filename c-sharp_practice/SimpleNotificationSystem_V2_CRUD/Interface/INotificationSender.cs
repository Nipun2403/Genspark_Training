using SimpleNotificationSystem_V2_CRUD.Models;

// No changes made here, same as last project
namespace SimpleNotificationSystem_V2_CRUD.Interface
{
  public interface INotificationSender
  {
    void SendNotification(User user, Notification notification);
  }
}