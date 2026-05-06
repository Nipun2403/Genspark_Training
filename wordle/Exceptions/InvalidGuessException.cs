using System;

namespace wordle.Exceptions
{
  public class InvalidGuessException : Exception
  {
    public InvalidGuessException(string message) : base(message)
    {
    }
  }
}