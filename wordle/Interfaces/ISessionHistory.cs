using System.Collections.Generic;
using wordle.Models;

namespace wordle.Interfaces
{
  public interface ISessionHistory
  {
    int TotalScore { get; }
    // new UserId in the add result for DB mapping
    void AddGameResult(int userId, GameResult result, int scoreEarned);
    void LoadUserHistory(int userId);
    void PrintHistoryTable(bool showTop3Only = false);
  }
}