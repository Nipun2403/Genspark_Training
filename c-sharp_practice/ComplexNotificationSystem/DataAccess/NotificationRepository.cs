using System.Collections.Generic;
using System.Linq;
using SharedModels;

// CRUD like functions for notifications.
namespace DataAccess
{
  public class NotificationRepository
  {
    private readonly List<NotificationLog> _sentNotifications = [];

    public void LogNotification(NotificationLog entry)
    {
      _sentNotifications.Add(entry);
    }

    public List<NotificationLog> GetNotificationsByID(int userId)
    {
      return [.. _sentNotifications.Where(n => n.UserId == userId)];
    }

    public List<NotificationLog> GetAllNotifications()
    {
      return [.. _sentNotifications];
    }
  }
}