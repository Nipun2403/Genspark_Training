// This interface is used to generate feedback based on player's guess. It will give the output in form of a string
namespace wordle.Interfaces
{
  public interface IFeedbackGenerator
  {
    string GenerateFeedback(string target, string guess);
  }
}