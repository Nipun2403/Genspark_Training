using System;
using System.Collections.Generic;
using wordle.Interfaces;
using wordle.Services;
using wordle.Exceptions;
using wordle.Models;

namespace wordle.Core
{
  public class GameEngine
  {
    private readonly IWordProvider _wordProvider;
    private readonly IGuessChecker _validator;
    private readonly IFeedbackGenerator _feedback;
    private readonly IHintManager _hintManager;
    private readonly ISessionHistory _sessionHistory;
    private readonly IComment _praiseProvider;
    private readonly ILeaderboardService _leaderboard;

    private readonly KeyboardTracker _keyboard;

    private HashSet<string> _previousGuesses;
    private List<string> _feedbackHistory;
    private List<string> _guessHistory;

    private User _currentUser = null!;


    public GameEngine(
        IWordProvider wordProvider,
        IGuessChecker validator,
        IFeedbackGenerator feedback,
        IHintManager hintManager,
        ISessionHistory sessionHistory,
        IComment praiseProvider,
        ILeaderboardService leaderboard)
    {
      _wordProvider = wordProvider;
      _validator = validator;
      _feedback = feedback;
      _hintManager = hintManager;
      _sessionHistory = sessionHistory;
      _praiseProvider = praiseProvider;
      _leaderboard = leaderboard;
      _keyboard = new KeyboardTracker();
      _previousGuesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _feedbackHistory = new List<string>();
      _guessHistory = new List<string>();
    }

    // Now returns a bool to tell Program.cs whether to keep running
    public bool Start(User user)
    {
      _currentUser = user;
      _sessionHistory.LoadUserHistory(_currentUser.Id);

      while (true)
      {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("======================================");
        Console.WriteLine($"      Welcome, {_currentUser.Username}!     ");
        Console.WriteLine("======================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("[1] Play Wordle");
        Console.WriteLine("[2] View Session History");
        Console.WriteLine("[3] Global Leaderboard");
        Console.WriteLine("[4] Rules of the Game");
        Console.WriteLine("[5] Logout (Return to Login)");
        Console.WriteLine("[6] Exit Application");
        Console.Write("\nEnter choice: ");

        string choice = Console.ReadLine()?.Trim() ?? string.Empty;

        switch (choice)
        {
          case "1":
            PlayGameLoop();
            break;
          case "2":
            ViewHistoryLoop(); // Extracted logic to handle the Top 3 filtering
            break;
          case "3":
            _leaderboard.PrintLeaderboard();
            break;
          case "4":
            PrintRules();
            break;
          case "5":
            Console.WriteLine("Logging out...");
            return true;
          case "6":
            return false;
          default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid option. Press ENTER to try again.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();
            break;
        }
      }
    }

    private void PlayGameLoop()
    {
      Console.Clear();
      Console.WriteLine("======================================");
      Console.WriteLine("            O F F -- B R A N D  W O R D L E : V2.0         ");
      Console.WriteLine("======================================\n");

      // Fetch difficulty, which might be NULL if they pressed '0' for Back
      Level? selectedDifficulty = SelectDifficulty();

      // The "Back" Catch. If null, immediately return to Main Menu.
      if (selectedDifficulty == null) return;

      int maxAttempts = 6;
      string targetWord = _wordProvider.GetRandomWord(selectedDifficulty.Value).ToUpper();
      string systemMessage = "";

      ResetGameState();

      int currentAttempt = 1;
      bool isVictory = false;

      while (currentAttempt <= maxAttempts)
      {
        DrawBoard();

        if (!string.IsNullOrEmpty(systemMessage))
        {
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.WriteLine($"\n>> {systemMessage}");
          Console.ForegroundColor = ConsoleColor.White;
          systemMessage = "";
        }

        // Type 0 here to rage quit back to the menu
        Console.Write($"\nAttempt {currentAttempt}/{maxAttempts} - Enter guess (or '?' Hint, '0' Back): ");
        string guess = Console.ReadLine()?.ToUpper() ?? string.Empty;

        if (guess == "0") return; // exit back to main menu

        if (guess == "?")
        {
          systemMessage = _hintManager.GetHint(targetWord);
          continue;
        }

        try
        {
          _validator.CheckGuess(guess, _previousGuesses);
          string feedbackResult = _feedback.GenerateFeedback(targetWord, guess);

          _previousGuesses.Add(guess);
          _guessHistory.Add(guess);
          _feedbackHistory.Add(feedbackResult);
          _keyboard.UpdateStates(guess, feedbackResult);

          if (feedbackResult == "GGGGG")
          {
            DrawBoard();
            Console.ForegroundColor = ConsoleColor.Cyan;
            string praise = _praiseProvider.GetComment(currentAttempt);
            Console.WriteLine($"\n{praise}");
            isVictory = true;
            break;
          }

          currentAttempt++;
        }
        catch (InvalidGuessException ex)
        {
          systemMessage = $"[ERROR] {ex.Message}";
        }
      }

      int pointsEarned = 0;
      if (isVictory)
      {
        pointsEarned = 100 - ((currentAttempt - 1) * 10);
      }
      else
      {
        DrawBoard();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nGame Over! The hidden word was: {targetWord}");
        Console.ForegroundColor = ConsoleColor.White;
      }

      _sessionHistory.AddGameResult(_currentUser.Id, new GameResult
      {
        TargetWord = targetWord,
        AttemptsTaken = currentAttempt,
        IsVictory = isVictory
      }, pointsEarned);

      _sessionHistory.PrintHistoryTable();

      Console.WriteLine("Press ENTER to return to the Main Menu...");
      Console.ReadLine();
    }

    // Return type is now nullable (Level?) to support the Back command
    private Level? SelectDifficulty()
    {
      while (true)
      {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Select Word Difficulty:");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("[1] Easy   (Common everyday words)");
        Console.WriteLine("[2] Medium (Standard vocabulary)");
        Console.WriteLine("[3] Hard   (Tricky words, repeating letters, rare characters)");
        Console.WriteLine("[0] Back to Main Menu");
        Console.Write("\nEnter choice (1/2/3/0): ");

        string input = Console.ReadLine()?.Trim().ToUpper() ?? string.Empty;

        if (input == "1") return Level.Easy;
        if (input == "2") return Level.Medium;
        if (input == "3") return Level.Hard;
        if (input == "0") return null; // Returns null if the user wants to go back

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[ERROR] Invalid selection. Please type 1, 2, 3, or 0.\n");
        Console.ForegroundColor = ConsoleColor.White;
      }
    }

    private void ResetGameState()
    {
      _previousGuesses.Clear();
      _feedbackHistory.Clear();
      _guessHistory.Clear();
      _keyboard.Reset();
      _hintManager.ResetForNewGame();
    }

    private void DrawBoard()
    {
      Console.Clear();
      Console.WriteLine("======================================");
      Console.WriteLine("            O F F -- B R A N D  W O R D L E : V2.0          ");
      Console.WriteLine("======================================");

      for (int i = 0; i < _guessHistory.Count; i++)
      {
        PrintColorizedFeedback(_guessHistory[i], _feedbackHistory[i]);
      }

      _keyboard.PrintKeyboard();
    }

    private void PrintColorizedFeedback(string guess, string feedback)
    {
      Console.Write("          ");
      for (int i = 0; i < 5; i++)
      {
        if (feedback[i] == 'G') Console.ForegroundColor = ConsoleColor.Green;
        else if (feedback[i] == 'Y') Console.ForegroundColor = ConsoleColor.Yellow;
        else Console.ForegroundColor = ConsoleColor.Red;

        Console.Write($"[{guess[i]}] ");
      }
      Console.WriteLine();
      Console.ForegroundColor = ConsoleColor.White;
    }

    private void PrintRules()
    {
      Console.Clear();
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("======================================");
      Console.WriteLine("          RULES OF THE GAME           ");
      Console.WriteLine("======================================");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("1. The system secretly chooses a 5-letter word.");
      Console.WriteLine("2. You have 6 attempts to guess it.");
      Console.WriteLine("3. After each guess, the letters change colors:");

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("   [GREEN]  Correct letter, correct spot.");

      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("   [YELLOW] Correct letter, wrong spot.");

      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("   [RED]    Letter not in the word.");

      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("\n4. Type '?' to use a Hint (Max 2 per round).");
      Console.WriteLine("5. Type '0' to rage-quit back to the main menu.");
      Console.WriteLine("======================================\n");
      Console.WriteLine("Press ENTER to return to the Main Menu...");
      Console.ReadLine();
    }

    private void ViewHistoryLoop()
    {
      Console.Clear();
      _sessionHistory.PrintHistoryTable(showTop3Only: false);

      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("[F] Filter Top 3 Scores   |   [ENTER] Return to Main Menu");
      Console.ForegroundColor = ConsoleColor.White;

      string input = Console.ReadLine()?.Trim().ToUpper() ?? "";

      if (input == "F")
      {
        Console.Clear();
        _sessionHistory.PrintHistoryTable(showTop3Only: true); // Print filtered!
        Console.WriteLine("Press ENTER to return to the Main Menu...");
        Console.ReadLine();
      }
    }
  }
}


