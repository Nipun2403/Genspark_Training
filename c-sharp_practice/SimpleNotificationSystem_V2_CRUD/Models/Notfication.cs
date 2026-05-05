using System;

// No changes made here, same as last project
namespace SimpleNotificationSystem_V2_CRUD.Models
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