using System;
using Npgsql;
using wordle.Interfaces;
using wordle.Models;

namespace wordle.Services
{
  public class WordProvider : IWordProvider
  {
    private readonly string _connectionString;

    public WordProvider(string connectionString)
    {
      _connectionString = connectionString;
    }

    public string GetRandomWord(Level difficulty)
    {
      using var conn = new NpgsqlConnection(_connectionString);
      conn.Open();

      // Fetching a random word with the difficulty the user selected using ORDER BY RANDOM()
      using var cmd = new NpgsqlCommand(
          "SELECT Word FROM Words WHERE Difficulty = @diff ORDER BY RANDOM() LIMIT 1", conn);
      cmd.Parameters.AddWithValue("diff", (int)difficulty);

      var result = cmd.ExecuteScalar();
      if (result == null)
        throw new Exception($"No words found in database for difficulty level {difficulty}");

      return result?.ToString() ?? string.Empty;
    }
  }
}