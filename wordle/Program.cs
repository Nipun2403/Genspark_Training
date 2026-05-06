using wordle.Core;
using wordle.Services;
using wordle.Interfaces;

// C# 9+ Top Level Statements

IWordProvider wordProvider = new WordProvider();
IGuessChecker guessValidator = new GuessChecker();
IFeedbackGenerator feedbackGenerator = new FeedbackGenerator();
IHintManager hintManager = new HintManager();
ISessionHistory sessionHistory = new SessionHistory();
IComment commentProvider = new Comment();

GameEngine game = new GameEngine(
    wordProvider,
    guessValidator,
    feedbackGenerator,
    hintManager,
    sessionHistory,
    commentProvider);

game.Start();