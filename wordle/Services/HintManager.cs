
using wordle.Interfaces;

// Implementaion of Hint manager Interface
namespace wordle.Services
{
  public class HintManager : IHintManager
  {
    // Store number of hints and the indices of the hint used.
    private int _hintsUsed;
    private readonly HashSet<int> _revealedIndices = new();
    private readonly Random _rand = new();


    public void ResetForNewGame()
    {
      _hintsUsed = 0;
      _revealedIndices.Clear();
    }

    // Limit for hints per game is 2. Anything more for a 5 letter word won't be fun at all.
    public string GetHint(string targetWord)
    {
      if (_hintsUsed >= 2)
        return "SYSTEM ALERT: Max hints (2/2) already used. You are on your own!";

      // indices of the word we haven't revealed yet
      var availableIndices = Enumerable.Range(0, 5).Where(i => !_revealedIndices.Contains(i)).ToList();

      // Gives out a random hint from the available indices
      int revealIndex = availableIndices[_rand.Next(availableIndices.Count)];

      _revealedIndices.Add(revealIndex);
      _hintsUsed++;

      return $"[HINT USED {_hintsUsed}/2]: The letter at position {revealIndex + 1} is '{targetWord[revealIndex]}'.";
    }
  }
}