using System;

namespace SharedModels
{
  public class NotificationUserJoin
  {
    public int LogId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
  }
}