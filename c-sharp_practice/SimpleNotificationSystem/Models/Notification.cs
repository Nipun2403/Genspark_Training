using System;

namespace SimpleNotificationSystem.Models
{
  public class Notification
  {
    // To avoid Null Error, string is initialized with empty string
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }

    public Notification(string message)
    {
      // Capturing the time when the notification is created
      Message = message;
      SentAt = DateTime.Now;
    }
  }
}