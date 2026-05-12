using System;
using Npgsql;
using wordle.Interfaces;
using wordle.Models;

namespace wordle.Services
{
  public class AuthService : IAuthService
  {
    private readonly string _connectionString;

    public AuthService(string connectionString)
    {
      _connectionString = connectionString;
    }

    public User LoginOrRegister(string username, string password)
    {
      using var conn = new NpgsqlConnection(_connectionString);
      conn.Open();

      // Check if user exists
      using var checkCmd = new NpgsqlCommand("SELECT Id, Password FROM Users WHERE Username = @u", conn);
      checkCmd.Parameters.AddWithValue("u", username);

      using var reader = checkCmd.ExecuteReader();
      if (reader.Read())
      {
        int id = reader.GetInt32(0);
        string dbPassword = reader.GetString(1);

        if (password != dbPassword)
          throw new Exception("Invalid password for existing user.");

        return new User { Id = id, Username = username };
      }
      reader.Close(); // close before executing

      // if User doesn't exist, create them  (JIT registration)
      using var insertCmd = new NpgsqlCommand(
          "INSERT INTO Users (Username, Password) VALUES (@u, @p) RETURNING Id", conn);
      insertCmd.Parameters.AddWithValue("u", username);
      insertCmd.Parameters.AddWithValue("p", password);

      int newId = Convert.ToInt32(insertCmd.ExecuteScalar());
      return new User { Id = newId, Username = username };
    }
  }
}