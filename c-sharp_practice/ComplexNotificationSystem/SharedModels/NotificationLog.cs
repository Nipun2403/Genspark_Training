namespace SharedModels
{

  public class NotificationLog
  {
    public int UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public Notification NotificationPayload { get; set; } = null!;
  }
}