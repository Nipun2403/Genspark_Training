using System;
using System.Collections.Generic;
using Npgsql;
using wordle.Interfaces;
using wordle.Models;
using System.Linq;

namespace wordle.Services
{
  public class SessionHistory : ISessionHistory
  {
    private readonly string _connectionString;
    private readonly List<GameResult> _history = new();
    public int TotalScore { get; private set; } = 0;

    public SessionHistory(string connectionString)
    {
      _connectionString = connectionString;
    }

    public void AddGameResult(int userId, GameResult result, int scoreEarned)
    {
      result.Score = scoreEarned;
      _history.Add(result);
      TotalScore += scoreEarned;

      using var conn = new NpgsqlConnection(_connectionString);
      conn.Open();

      using var cmd = new NpgsqlCommand(
          "INSERT INTO GameSessions (UserId, TargetWord, AttemptsTaken, IsVictory, ScoreEarned) " +
          "VALUES (@uid, @word, @attempts, @win, @score)", conn);

      cmd.Parameters.AddWithValue("uid", userId);
      cmd.Parameters.AddWithValue("word", result.TargetWord);
      cmd.Parameters.AddWithValue("attempts", result.AttemptsTaken);
      cmd.Parameters.AddWithValue("win", result.IsVictory);
      cmd.Parameters.AddWithValue("score", scoreEarned);

      cmd.ExecuteNonQuery();
    }

    public void LoadUserHistory(int userId)
    {
      _history.Clear();
      TotalScore = 0;

      using var conn = new NpgsqlConnection(_connectionString);
      conn.Open();

      using var cmd = new NpgsqlCommand(
          "SELECT TargetWord, AttemptsTaken, IsVictory, ScoreEarned FROM GameSessions WHERE UserId = @uid ORDER BY PlayedAt ASC", conn);
      cmd.Parameters.AddWithValue("uid", userId);

      using var reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        var result = new GameResult
        {
          TargetWord = reader.GetString(0),
          AttemptsTaken = reader.GetInt32(1),
          IsVictory = reader.GetBoolean(2),
          Score = reader.GetInt32(3)
        };

        _history.Add(result);
        TotalScore += result.Score;
      }
    }

    public void PrintHistoryTable(bool showTop3Only = false)
    {
      Console.WriteLine("\n=================================================");
      Console.WriteLine(showTop3Only ? "       TOP 3 SCORES       " : "             SESSION HISTORY           ");
      Console.WriteLine("=================================================");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"TOTAL ACCUMULATED SCORE: {TotalScore}\n");

      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine(String.Format("{0,-12} | {1,-10} | {2,-8} | {3}", "TARGET WORD", "ATTEMPTS", "STATUS", "SCORE"));
      Console.WriteLine(new string('-', 49));

      // Apply the Top 3 filter dynamically using LINQ
      var displayList = showTop3Only
          ? _history.OrderByDescending(g => g.Score).Take(3).ToList()
          : _history;

      foreach (var game in displayList)
      {
        Console.ForegroundColor = game.IsVictory ? ConsoleColor.Green : ConsoleColor.Red;
        string status = game.IsVictory ? "CLEARED" : "FAILED";
        string attempts = game.IsVictory ? game.AttemptsTaken.ToString() : "X";
        string scoreText = $"+{game.Score}";

        Console.WriteLine(String.Format("{0,-12} | {1,-10} | {2,-8} | {3}", game.TargetWord, attempts, status, scoreText));
      }

      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("=================================================\n");
    }
  }
}