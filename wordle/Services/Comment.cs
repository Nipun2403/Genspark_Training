using System.Collections.Generic;
using wordle.Interfaces;

namespace wordle.Services
{
  public class Comment : IComment
  {
    private readonly Dictionary<int, string> _praiseMatrix;

    public Comment()
    {
      // The data is now safely isolated in its own domain
      _praiseMatrix = new Dictionary<int, string>
            {
                { 1, "Genius!" },
                { 2, "Excellent!" },
                { 3, "Great job!" },
                { 4, "Good work!" },
                { 5, "Nice try!" },
                { 6, "That was close!" }
            };
    }

    public string GetComment(int attemptNumber)
    {
      // Returns the specific comment, or a default fallback if the attempt number is somehow out of bounds
      return _praiseMatrix.GetValueOrDefault(attemptNumber, "Wow!");
    }
  }
}