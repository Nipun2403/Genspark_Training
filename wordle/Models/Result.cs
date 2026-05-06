
// Used to store and show the result.
namespace wordle.Models
{
  public class GameResult
  {
    public string TargetWord { get; set; } = string.Empty;
    public int AttemptsTaken { get; set; }
    public bool IsVictory { get; set; }

    public int Score { get; set; }
  }
}