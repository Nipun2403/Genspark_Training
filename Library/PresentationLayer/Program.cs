using DataAccessLayer;
using BusinessLogicLayer.Services;

namespace PresentationLayer;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║     Library Management System         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Create a single DbContext instance for the application
        using var context = new LibraryDbContext();

        // Initialize database automatically (creates database, tables, and stored functions)
        Console.WriteLine("Initializing local database connection...");
        try
        {
            context.InitializeDatabase();
            Console.WriteLine("Database, tables, and PostgreSQL stored functions initialized successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("DATABASE INITIALIZATION WARNING");
            Console.ResetColor();
            Console.WriteLine($"Error detail: {ex.Message}");
            Console.WriteLine("Please ensure PostgreSQL is running and the connection string in LibraryDbContext.cs is correct.");
            Console.WriteLine("Continuing application startup...");
            Console.WriteLine();
        }

        // Initialize services (simple instantiation — no DI container needed)
        var memberService = new MemberService(context);
        var bookService = new BookService(context);
        var borrowingService = new BorrowingService(context);
        var returnService = new ReturnService(context);
        var fineService = new FineService(context);
        var reportService = new ReportService(context);

        // Create menu handlers
        var memberMenu = new Menus.MemberMenu(memberService);
        var bookMenu = new Menus.BookMenu(bookService);
        var borrowMenu = new Menus.BorrowMenu(borrowingService, memberService, bookService);
        var returnMenu = new Menus.ReturnMenu(returnService, borrowingService);
        var fineMenu = new Menus.FineMenu(fineService, memberService);
        var reportMenu = new Menus.ReportMenu(reportService, memberService);

        bool running = true;

        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║           MAIN MENU                 ║");
            Console.WriteLine("╠══════════════════════════════════════╣");
            Console.WriteLine("║  1. Member Management               ║");
            Console.WriteLine("║  2. Book Management                 ║");
            Console.WriteLine("║  3. Borrow Book                     ║");
            Console.WriteLine("║  4. Return Book                     ║");
            Console.WriteLine("║  5. Fine Management                 ║");
            Console.WriteLine("║  6. Reports                         ║");
            Console.WriteLine("║  7. Exit                            ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.Write("Select an option: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await memberMenu.ShowAsync();
                    break;
                case "2":
                    await bookMenu.ShowAsync();
                    break;
                case "3":
                    await borrowMenu.ShowAsync();
                    break;
                case "4":
                    await returnMenu.ShowAsync();
                    break;
                case "5":
                    await fineMenu.ShowAsync();
                    break;
                case "6":
                    await reportMenu.ShowAsync();
                    break;
                case "7":
                    running = false;
                    Console.WriteLine("Goodbye!");
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
