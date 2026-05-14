namespace SharedModels
{

  public class NotificationLog
  {
    public int LogId { get; set; }
    public int UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }

    // one Notififaotin one user
    public User User { get; set; } = null!;
  }
}