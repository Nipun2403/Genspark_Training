using System;
using System.Collections.Generic;


// This will map the keyboard on the console and keep track of the state of each letter based on guess
// Same color schemes : G -> Green, Y -> Yellow, R -> Red, U -> Unused
namespace wordle.Services
{
  public class KeyboardTracker
  {
    private readonly Dictionary<char, char> _keyStates = new();

    // Keyboard layout of my laptop
    private readonly string[] _keyboardLayout = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };

    public KeyboardTracker()
    {
      Reset();
    }

    public void Reset()
    {
      for (char c = 'A'; c <= 'Z'; c++)
        _keyStates[c] = 'U'; // 'U' for Unused
    }

    public void UpdateStates(string guess, string feedback)
    {
      for (int i = 0; i < 5; i++)
      {
        char letter = guess[i];
        char result = feedback[i];

        // If already correct guess (Green), skip
        if (_keyStates[letter] == 'G') continue;
        // If already yellow and now guessed wrong (Red), skip 
        if (_keyStates[letter] == 'Y' && result == 'R') continue;

        _keyStates[letter] = result;
      }
    }

    // Print the keyboard with color based on the guesses
    public void PrintKeyboard()
    {
      Console.WriteLine("\n=== KEYBOARD ===");
      foreach (string row in _keyboardLayout)
      {
        // Padding to make the keyboard not look wonky or crippled
        if (row.StartsWith("A")) Console.Write(" ");
        if (row.StartsWith("Z")) Console.Write("  ");

        foreach (char key in row)
        {
          char state = _keyStates[key];

          if (state == 'G') Console.ForegroundColor = ConsoleColor.Green;
          else if (state == 'Y') Console.ForegroundColor = ConsoleColor.Yellow;
          else if (state == 'R') Console.ForegroundColor = ConsoleColor.Red;
          else Console.ForegroundColor = ConsoleColor.White;

          Console.Write($"{key} ");
        }
        Console.WriteLine();
      }
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("================\n");
    }
  }
}