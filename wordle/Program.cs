using System;
using wordle.Core;
using wordle.Services;
using wordle.Interfaces;
using wordle.Models;

string connectionString = "Host=localhost;Database=wordle;Username=peewee;Password=";

IAuthService authService = new AuthService(connectionString);
IWordProvider wordProvider = new WordProvider(connectionString);
IGuessChecker guessValidator = new GuessChecker();
IFeedbackGenerator feedbackGenerator = new FeedbackGenerator();
IHintManager hintManager = new HintManager();
ISessionHistory sessionHistory = new SessionHistory(connectionString);
IComment praiseProvider = new Comment();
ILeaderboardService leaderboard = new LeaderboardService(connectionString);

GameEngine game = new GameEngine(
    wordProvider,
    guessValidator,
    feedbackGenerator,
    hintManager,
    sessionHistory,
    praiseProvider, leaderboard);

bool appRunning = true;

// THE OUTER APPLICATION LOOP
while (appRunning)
{
    Console.Clear();
    Console.WriteLine("======================================");
    Console.WriteLine("      LOGIN PORTAL       ");
    Console.WriteLine("======================================");
    Console.WriteLine("Type '0' as  to quit.\n");

    User? loggedInUser = null;

    while (loggedInUser == null)
    {
        Console.Write("Username: ");
        string username = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;

        // backdoor to close the app from the login screen
        if (username.Equals("0", StringComparison.OrdinalIgnoreCase))
        {
            appRunning = false;
            break;
        }

        Console.Write("Password: ");
        // string password = Console.ReadLine()?.Trim() ?? string.Empty;
        string password = ReadPassword().Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[WARNING] Username and Password cannot be empty. Nice try though.\n");
            Console.ForegroundColor = ConsoleColor.White;
            continue;
        }

        try
        {
            loggedInUser = authService.LoginOrRegister(username, password);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[LOGIN FAILED] {ex.Message}\n");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    // If a user successfully logged in, hand them to the Game Engine.
    // The engine will return TRUE if they logged out, and FALSE if they hit Exit.
    if (loggedInUser != null)
    {
        appRunning = game.Start(loggedInUser);
    }

    // Drop this at the bottom of Program.cs
    string ReadPassword()
    {
        string password = "";
        while (true)
        {
            // 'true' intercepts the keypress so it doesn't print to the console
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine(); // Move to the next line after hitting Enter
                break;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                // If they hit backspace, remove the last char from the string AND erase the * from the screen
                if (password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                // Add the real character to our secret string, but print a fake * to the screen
                password += key.KeyChar;
                Console.Write("*");
            }
        }
        return password;
    }
}

Console.WriteLine("\nShutting down the game. Goodbye!");