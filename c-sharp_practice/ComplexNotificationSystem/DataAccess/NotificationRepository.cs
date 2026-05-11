using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using SharedModels;
using SharedModels.Interfaces;

namespace DataAccess
{
  public class NotificationRepository : INotificationRepository
  {
    private readonly string _connectionString;

    public NotificationRepository(string connectionString)
    {
      _connectionString = connectionString;
    }

    public async Task SaveAsync(NotificationLog log)
    {
      using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync();

      string sql = "INSERT INTO NotificationLogs (UserId, NotificationType, Message, SentAt) VALUES (@UserId, @Type, @Message, @SentAt);";
      using var cmd = new NpgsqlCommand(sql, conn);
      cmd.Parameters.AddWithValue("@UserId", log.UserId);
      cmd.Parameters.AddWithValue("@Type", log.NotificationType);
      cmd.Parameters.AddWithValue("@Message", log.Message);
      cmd.Parameters.AddWithValue("@SentAt", log.SentAt);

      await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<NotificationUserJoin>> GetJoinedNotificationHistoryAsync()
    {
      var history = new List<NotificationUserJoin>();
      using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync();

      string sql = @"
                SELECT nl.LogId, u.Name, u.Email, nl.NotificationType, nl.Message, nl.SentAt
                FROM NotificationLogs nl
                INNER JOIN Users u ON nl.UserId = u.Id
                ORDER BY nl.SentAt DESC;";

      using var cmd = new NpgsqlCommand(sql, conn);
      using var reader = await cmd.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        history.Add(new NotificationUserJoin
        {
          LogId = reader.GetInt32(0),
          UserName = reader.GetString(1),
          UserEmail = reader.IsDBNull(2) ? "N/A" : reader.GetString(2),
          NotificationType = reader.GetString(3),
          Message = reader.GetString(4),
          SentAt = reader.GetDateTime(5)
        });
      }
      return history;
    }
  }
}