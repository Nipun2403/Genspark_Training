using wordle.Interfaces;

// Acutal Class that inherits the feedback generator interface and implements the method
namespace wordle.Services
{
  public class FeedbackGenerator : IFeedbackGenerator
  {
    public string GenerateFeedback(string target, string guess)
    {
      // Stores feedback for each letter: 'G' for Green, 'Y' for Yellow, "R" for Red)
      char[] feedback = new char[5];
      bool[] targetUsed = new bool[5];

      // Check 1: Correct letters in correct positions (Green)
      for (int i = 0; i < 5; i++)
      {
        if (guess[i] == target[i])
        {
          feedback[i] = 'G';
          targetUsed[i] = true;
        }
      }

      // Check 2: Correct letters in wrong positions (Yellow)
      for (int i = 0; i < 5; i++)
      {
        if (feedback[i] == 'G') continue; // Skip already matched letters

        feedback[i] = 'R'; // Default and prefill to Red (incorrect)

        for (int j = 0; j < 5; j++)
        {
          if (!targetUsed[j] && guess[i] == target[j])
          {
            feedback[i] = 'Y'; // Mark as Yellow (correct letter, wrong position)
            targetUsed[j] = true;
            break;
          }
        }

      }
      return new string(feedback);

    }
  }
}