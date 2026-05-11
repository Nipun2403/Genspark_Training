using System.Threading.Tasks;

namespace SharedModels.Interfaces
{
  public interface INotificationSender
  {
    Task SendNotificationAsync(User user, string message);
  }
}