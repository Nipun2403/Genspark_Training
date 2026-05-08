using System;

namespace SharedModels
{

  public class Notification
  {
    public string Message { get; set; }
    public DateTime SentAt { get; set; }

    public Notification(string message, DateTime sentAt)
    {
      Message = message;
      SentAt = sentAt;
    }
  }
}