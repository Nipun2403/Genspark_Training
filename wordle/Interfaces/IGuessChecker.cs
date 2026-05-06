using System.Collections.Generic;

// This interface will be used to check weather the player input guess is valid or not, based on various constrains that will be defined in the class.
namespace wordle.Interfaces
{
  public interface IGuessChecker
  {

    // using 
    void CheckGuess(string guess, HashSet<string> previousGuesses);
  }
}