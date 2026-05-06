using System;
using System.Collections.Generic;
using wordle.Interfaces;
using wordle.Models;

// Session History Implementation
namespace wordle.Services
{
  public class SessionHistory : ISessionHistory
  {
    // Stores the history in a list of GameResult Objects
    private readonly List<GameResult> _history = new();
    public int TotalScore { get; private set; } = 0;

    public void AddGameResult(GameResult result, int scoreEarned)
    {
      result.Score = scoreEarned;
      _history.Add(result);
      TotalScore += scoreEarned;
    }

    public void PrintHistoryTable()
    {
      // Random Console Prints for Session History 
      Console.WriteLine("\n=================================================");
      Console.WriteLine("             GAME PROGRESS HISTORY            ");
      Console.WriteLine("=================================================");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"TOTAL ACCUMULATED SCORE: {TotalScore}\n");

      // C# String formatting for table columns
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine(String.Format("{0,-12} | {1,-10} | {2,-8} | {3}", "TARGET WORD", "ATTEMPTS", "STATUS", "SCORE"));
      Console.WriteLine(new string('-', 49));


      foreach (var game in _history)
      {
        // The Stakeholder Requirement: Green row for win, Red row for loss
        Console.ForegroundColor = game.IsVictory ? ConsoleColor.Green : ConsoleColor.Red;

        string status = game.IsVictory ? "CLEARED" : "FAILED";
        string attempts = game.IsVictory ? game.AttemptsTaken.ToString() : "X";

        string scoreText = $"+{game.Score}";

        Console.WriteLine(String.Format("{0,-12} | {1,-10} | {2,-8} | {3}", game.TargetWord, attempts, status, scoreText));
      }

      Console.ForegroundColor = ConsoleColor.White; // Always reset!
      Console.WriteLine("======================================\n");
    }
  }
}