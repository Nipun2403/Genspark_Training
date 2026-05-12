namespace wordle.Models
{
  // To show the leaderboard
  public class LeaderboardEntry
  {
    public int Rank { get; set; }
    public string Username { get; set; } = string.Empty;
    public int TotalScore { get; set; }
  }
}