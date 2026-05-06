using System.Collections.Generic;
using wordle.Interfaces;
using wordle.Exceptions;

// Class Implements the Guess Checker interface.
namespace wordle.Services
{
  public class GuessChecker : IGuessChecker
  {

    // Using Hashset for quick lookup of previous guesses. O(1) lookup time.
    public void CheckGuess(string guess, HashSet<string> previousGuesses)
    {

      // If empty or whitespace, give error 
      if (string.IsNullOrWhiteSpace(guess))
      {
        throw new InvalidGuessException("Empty Guess? Really?? Try again.");
      }
      //  If out of length, give error
      if (guess.Length != 5)
      {
        throw new InvalidGuessException("5 letters only, come on. Try again.");
      }
      // If contains anythign other than characters, give error
      if (!guess.All(char.IsLetter))
      {
        throw new InvalidGuessException("Ayoo, only letters please.");
      }
      // If already guessed, give error
      if (previousGuesses.Contains(guess))
      {
        throw new InvalidGuessException("You already guessed that word! Try something new.");
      }
    }
  }
}

