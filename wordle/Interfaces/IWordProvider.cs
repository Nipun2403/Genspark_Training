using wordle.Models;
// Interface for providing random words.
namespace wordle.Interfaces
{
  public interface IWordProvider
  {
    string GetRandomWord(Level level);
  }
}