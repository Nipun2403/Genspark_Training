namespace SharedModels.Interfaces
{
  // The polymorphic contract. Any notification channel must implement this.
  public interface INotificationSender
  {
    void SendNotification(User user, Notification notification);
  }
}