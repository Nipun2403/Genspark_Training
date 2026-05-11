using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedModels.Interfaces
{
  public interface INotificationRepository
  {
    Task SaveAsync(NotificationLog log);
    Task<List<NotificationUserJoin>> GetJoinedNotificationHistoryAsync();
  }
}