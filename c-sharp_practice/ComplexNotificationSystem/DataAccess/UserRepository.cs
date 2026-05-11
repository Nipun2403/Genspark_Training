using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using SharedModels;
using SharedModels.Interfaces;

namespace DataAccess
{
  public class UserRepository : IUserRepository
  {
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
      _connectionString = connectionString;
    }

    public async Task<User> AddUserAsync(User user)
    {
      using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync();

      string sql = "INSERT INTO Users (Name, Email, PhoneNumber) VALUES (@Name, @Email, @Phone) RETURNING Id;";
      using var cmd = new NpgsqlCommand(sql, conn);
      cmd.Parameters.AddWithValue("@Name", user.Name);
      cmd.Parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
      cmd.Parameters.AddWithValue("@Phone", (object?)user.PhoneNumber ?? DBNull.Value);

      user.Id = (int)(await cmd.ExecuteScalarAsync() ?? 0);
      return user;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
      var users = new List<User>();
      using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync();

      using var cmd = new NpgsqlCommand("SELECT Id, Name, Email, PhoneNumber FROM Users;", conn);
      using var reader = await cmd.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        users.Add(new User
        {
          Id = reader.GetInt32(0),
          Name = reader.GetString(1),
          Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
          PhoneNumber = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
        });
      }
      return users;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
      using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync();

      using var cmd = new NpgsqlCommand("SELECT Id, Name, Email, PhoneNumber FROM Users WHERE Id = @Id;", conn);
      cmd.Parameters.AddWithValue("@Id", id);

      using var reader = await cmd.ExecuteReaderAsync();
      if (await reader.ReadAsync())
      {
        return new User
        {
          Id = reader.GetInt32(0),
          Name = reader.GetString(1),
          Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
          PhoneNumber = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
        };
      }
      throw new InvalidOperationException("User not found");
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
      using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync();

      string sql = "UPDATE Users SET Name = @Name, Email = @Email, PhoneNumber = @Phone WHERE Id = @Id;";
      using var cmd = new NpgsqlCommand(sql, conn);
      cmd.Parameters.AddWithValue("@Id", user.Id);
      cmd.Parameters.AddWithValue("@Name", user.Name);
      cmd.Parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
      cmd.Parameters.AddWithValue("@Phone", (object?)user.PhoneNumber ?? DBNull.Value);

      return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
      using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync();

      using var cmd = new NpgsqlCommand("DELETE FROM Users WHERE Id = @Id;", conn);
      cmd.Parameters.AddWithValue("@Id", id);

      return await cmd.ExecuteNonQueryAsync() > 0;
    }
  }
}