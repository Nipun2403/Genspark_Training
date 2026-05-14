using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using SharedModels.Interfaces;

namespace DataAccess
{
  public class NotificationRepository : INotificationRepository
  {
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
      _context = context;
    }

    public async Task SaveAsync(NotificationLog log)
    {
      _context.NotificationLogs.Add(log);
      await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationUserJoin>> GetJoinedNotificationHistoryAsync()
    {
      // EF Core translates LINQ 
      return await _context.NotificationLogs
          .AsNoTracking()
          .Include(n => n.User) // EF Core JOIN the Users table
          .OrderByDescending(n => n.SentAt)
          .Select(n => new NotificationUserJoin
          {
            LogId = n.LogId,
            UserName = n.User.Name,
            UserEmail = string.IsNullOrEmpty(n.User.Email) ? "N/A" : n.User.Email,
            NotificationType = n.NotificationType,
            Message = n.Message,
            SentAt = n.SentAt
          })
          .ToListAsync();
    }
  }
}