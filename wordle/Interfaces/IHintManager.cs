
// used to give hints to player. Also keep track of how many hitns used
// It will reset hint count when player exits the session
namespace wordle.Interfaces
{
  public interface IHintManager
  {
    void ResetForNewGame();
    string GetHint(string targetWord);
  }
}