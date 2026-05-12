using System;
using Npgsql;
using wordle.Interfaces;

namespace wordle.Services
{
  public class LeaderboardService : ILeaderboardService
  {
    private readonly string _connectionString;

    public LeaderboardService(string connectionString)
    {
      _connectionString = connectionString;
    }

    public void PrintLeaderboard()
    {
      Console.Clear();
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("=================================================");
      Console.WriteLine("             GLOBAL LEADERBOARD              ");
      Console.WriteLine("=================================================");
      Console.ForegroundColor = ConsoleColor.White;

      Console.WriteLine(String.Format("{0,-6} | {1,-20} | {2}", "RANK", "USERNAME", "TOTAL SCORE"));
      Console.WriteLine(new string('-', 49));

      using var conn = new NpgsqlConnection(_connectionString);
      conn.Open();

      // Join Users and GameSessions, Sum the scores, and sort them descending.
      string sql = @"
                SELECT u.Username, COALESCE(SUM(g.ScoreEarned), 0) as TotalScore
                FROM Users u
                JOIN GameSessions g ON u.Id = g.UserId
                GROUP BY u.Username
                ORDER BY TotalScore DESC
                LIMIT 10;"; // Only pull the top 10 to keep the console from becoming a messy blob of text

      using var cmd = new NpgsqlCommand(sql, conn);
      using var reader = cmd.ExecuteReader();

      int rank = 1;
      bool hasData = false;

      while (reader.Read())
      {
        hasData = true;
        string username = reader.GetString(0);
        int score = reader.GetInt32(1);

        // Add some visual jazz to the top 3 players
        if (rank == 1) Console.ForegroundColor = ConsoleColor.Yellow;
        else if (rank == 2) Console.ForegroundColor = ConsoleColor.DarkBlue;
        else if (rank == 3) Console.ForegroundColor = ConsoleColor.DarkRed;
        else Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine(String.Format("#{0,-5} | {1,-20} | {2}", rank, username, score));
        rank++;
      }

      if (!hasData)
      {
        Console.WriteLine("No games played yet. The throne is empty!");
      }

      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("=================================================\n");
      Console.WriteLine("Press ENTER to return to the Main Menu...");
      Console.ReadLine();
    }
  }
}