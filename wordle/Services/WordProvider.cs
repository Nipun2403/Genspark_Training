using System;
using System.Collections.Generic;
using wordle.Interfaces;
using wordle.Models;

namespace wordle.Services
{
  public class WordProvider : IWordProvider
  {
    private readonly Dictionary<Level, List<string>> _wordBank;
    private readonly Random _rand;

    public WordProvider()
    {
      _rand = new Random();

      // Enterprise Dictionary mapping Enums to categorized word lists
      _wordBank = new Dictionary<Level, List<string>>
            {
                {
                    Level.Easy,
                    new List<string> { "APPLE", "HOUSE", "WATER", "CHAIR", "TABLE" }
                },
                {
                    Level.Medium,
                    new List<string> { "PLANT", "TRAIN", "BRAIN", "SMART", "WORLD", "MANGO" }
                },
                { 
                    // Hard words have repeating letters, tricky placements, or rare characters (Z, X, Q, V, Y)
                    Level.Hard,
                    new List<string> { "JAZZY", "QUELL", "VIVID", "ZESTY", "CRYPT", "FLUFF" }
                }
            };
    }

    public string GetRandomWord(Level level)
    {
      // Fetch the specific list based on the user's choice
      var wordList = _wordBank[level];
      return wordList[_rand.Next(wordList.Count)];
    }
  }
}