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
    private readonly IComment _commentProvider;

    private readonly KeyboardTracker _keyboard;

    private HashSet<string> _previousGuesses;
    private List<string> _feedbackHistory;
    private List<string> _guessHistory;

    public GameEngine(
        IWordProvider wordProvider,
        IGuessChecker validator,
        IFeedbackGenerator feedback,
        IHintManager hintManager,
        ISessionHistory sessionHistory,
        IComment commentProvider)
    {
      _wordProvider = wordProvider;
      _validator = validator;
      _feedback = feedback;
      _hintManager = hintManager;
      _sessionHistory = sessionHistory;
      _commentProvider = commentProvider;

      _keyboard = new KeyboardTracker();
      _previousGuesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _feedbackHistory = new List<string>();
      _guessHistory = new List<string>();
    }

    public void Start()
    {
      bool playAgain = true;

      while (playAgain)
      {
        Level selectedDifficulty = SelectDifficulty();
        int maxAttempts = 6;
        string targetWord = _wordProvider.GetRandomWord(selectedDifficulty).ToUpper();
        string systemMessage = ""; // For errors or hints 

        ResetGameState();

        int currentAttempt = 1;
        bool isVictory = false;

        while (currentAttempt <= maxAttempts)
        {
          DrawBoard();

          // Display any hints or errors from the previous  attempt
          if (!string.IsNullOrEmpty(systemMessage))
          {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n>> {systemMessage}");
            Console.ForegroundColor = ConsoleColor.White;
            systemMessage = ""; // clear after displaying
          }

          Console.Write($"\nAttempt {currentAttempt}/{maxAttempts} - Enter guess (or '?' for Hint): ");
          string guess = Console.ReadLine()?.ToUpper() ?? string.Empty;

          // If user choose hint, run skip checking and feedback
          if (guess == "?")
          {
            systemMessage = _hintManager.GetHint(targetWord);
            continue; // Skip checking, redraw with the new hint
          }

          try
          {
            _validator.CheckGuess(guess, _previousGuesses);
            string feedbackResult = _feedback.GenerateFeedback(targetWord, guess);

            _previousGuesses.Add(guess);
            _guessHistory.Add(guess);
            _feedbackHistory.Add(feedbackResult);
            _keyboard.UpdateStates(guess, feedbackResult);

            // Winning condition, all correct
            if (feedbackResult == "GGGGG")
            {
              DrawBoard();
              Console.ForegroundColor = ConsoleColor.Cyan;
              string comment = _commentProvider.GetComment(currentAttempt);
              Console.WriteLine($"\n{comment}");

              isVictory = true;
              break;
            }

            currentAttempt++;
          }
          catch (InvalidGuessException ex)
          {
            // Make the error look more "system alert"-y and less like a user typo
            systemMessage = $"[ERROR] {ex.Message}";
          }
        }

        // Calculate Score and Save to History
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

        _sessionHistory.AddGameResult(new GameResult
        {
          TargetWord = targetWord,
          AttemptsTaken = currentAttempt,
          IsVictory = isVictory
        }, pointsEarned);

        // Print the new History Table
        _sessionHistory.PrintHistoryTable();

        Console.Write("Play another round? (Y/N): ");
        playAgain = Console.ReadLine()?.Trim().ToUpper() == "Y";
      }

      Console.WriteLine($"\nBye Bye! Final Score: {_sessionHistory.TotalScore}. Good work.");
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
      Console.WriteLine("==================================================");
      Console.WriteLine("            O F F -- B R A N D  W O R D L E           ");
      Console.WriteLine("==================================================");

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

    private Level SelectDifficulty()
    {
      while (true)
      {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Select Word Difficulty:");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("[1] Easy ");
        Console.WriteLine("[2] Medium");
        Console.WriteLine("[3] Hard  ");
        Console.Write("\nEnter choice (1/2/3): ");

        string input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (input == "1") return Level.Easy;
        if (input == "2") return Level.Medium;
        if (input == "3") return Level.Hard;

        // Enterprise error handling
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[ERROR] Invalid selection. Please type 1, 2, or 3.\n");
        Console.ForegroundColor = ConsoleColor.White;
      }
    }
  }
}