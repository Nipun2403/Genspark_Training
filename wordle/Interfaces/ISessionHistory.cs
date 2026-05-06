using wordle.Models;

// This will keep track of game history in current session.
namespace wordle.Interfaces
{
  public interface ISessionHistory
  {
    int TotalScore { get; }
    void AddGameResult(GameResult result, int scoreEarned);
    void PrintHistoryTable();
  }
}