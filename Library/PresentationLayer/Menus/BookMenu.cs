using BusinessLogicLayer.Services;
using PresentationLayer.UI;

namespace PresentationLayer.Menus;


/// Console menu for book and book copy management.

public class BookMenu
{
    private readonly BookService _bookService;

    public BookMenu(BookService bookService)
    {
        _bookService = bookService;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("--- BOOK MANAGEMENT ---");
            Console.WriteLine("1. Add Category");
            Console.WriteLine("2. Add New Book");
            Console.WriteLine("3. Add Copies of a Book");
            Console.WriteLine("4. View Books Inventory");
            Console.WriteLine("5. Search Books");
            Console.WriteLine("6. View Copies of a Book");
            Console.WriteLine("7. Mark Copy Status");
            Console.WriteLine("0. Back to Main Menu");
            
            var choice = InputValidator.GetString("Select: ");
            switch (choice)
            {
                case "1": await AddCategoryAsync(); break;
                case "2": await AddBookAsync(); break;
                case "3": await AddCopiesAsync(); break;
                case "4": await ViewBooksInventoryAsync(); break;
                case "5": await SearchBooksAsync(); break;
                case "6": await ViewCopiesAsync(); break;
                case "7": await MarkCopyStatusAsync(); break;
                case "0": back = true; break;
                default: Console.WriteLine("  [Error] Invalid option."); break;
            }
        }
    }

    private async Task AddCategoryAsync()
    {
        Console.WriteLine("\n--- Add Category ---");
        var name = InputValidator.GetValidName("Category Name: ");
        var result = await _bookService.AddCategoryAsync(name);
        Console.WriteLine(result);
    }

    private async Task AddBookAsync()
    {
        Console.WriteLine("\n--- Add New Book ---");
        var isbn = InputValidator.GetString("ISBN: ");
        
        // Let's use GetString for title and author as they might contain numbers (e.g. "Catch-22", "1984")
        var title = InputValidator.GetString("Title: ");
        var author = InputValidator.GetValidName("Author: ");

        var categories = await _bookService.GetAllCategoriesAsync();
        if (categories.Count == 0)
        {
            Console.WriteLine("No categories exist. Please add a category first.");
            return;
        }

        var category = InputValidator.GetSelection(categories, "Select Category: ", c => c.CategoryName);
        if (category == null) return;

        var result = await _bookService.AddBookAsync(isbn, title, author, category.CategoryId);
        Console.WriteLine(result);
    }

    private async Task AddCopiesAsync()
    {
        Console.WriteLine("\n--- Add Copies ---");
        var books = await _bookService.GetAllBooksAsync();
        if (books.Count == 0)
        {
            Console.WriteLine("No books found in the library. Add a book first.");
            return;
        }

        var book = InputValidator.GetSelection(books, "Select Book: ", b => $"{b.Title} (ISBN: {b.ISBN})");
        if (book == null) return;

        var count = InputValidator.GetValidInt($"Number of copies to add for '{book.Title}': ", min: 1);
        var result = await _bookService.AddCopiesAsync(book.ISBN, count);
        Console.WriteLine(result);
    }

    private async Task ViewBooksInventoryAsync()
    {
        var books = await _bookService.GetAllBooksAsync();
        if (books.Count == 0)
        {
            Console.WriteLine("No books found.");
            return;
        }

        Console.WriteLine($"\n{"ISBN",-18} {"Title",-30} {"Author",-20} {"Category",-15} {"Copies (Avail/Total)"}");
        Console.WriteLine(new string('-', 105));
        foreach (var b in books)
        {
            int totalCount = b.Copies.Count;
            int availCount = b.Copies.Count(c => c.Status == "Available" || c.Status == "MinorDamage");
            
            Console.WriteLine($"{b.ISBN,-18} {b.Title,-30} {b.Author,-20} {b.Category.CategoryName,-15} {availCount}/{totalCount}");
        }
    }

    private async Task SearchBooksAsync()
    {
        Console.WriteLine("\n--- Search Books ---");
        var search = InputValidator.GetString("Search (title/author/category/ISBN): ");

        var books = await _bookService.SearchBooksAsync(search);
        if (books.Count == 0)
        {
            Console.WriteLine("No books found.");
            return;
        }

        Console.WriteLine($"\n{"ISBN",-18} {"Title",-30} {"Author",-20} {"Category",-15}");
        Console.WriteLine(new string('-', 85));
        foreach (var b in books)
        {
            Console.WriteLine($"{b.ISBN,-18} {b.Title,-30} {b.Author,-20} {b.Category.CategoryName,-15}");
        }
    }

    private async Task ViewCopiesAsync()
    {
        Console.WriteLine("\n--- View Copies ---");
        var books = await _bookService.GetAllBooksAsync();
        if (books.Count == 0)
        {
            Console.WriteLine("No books found in the library.");
            return;
        }

        var book = InputValidator.GetSelection(books, "Select Book: ", b => $"{b.Title} (ISBN: {b.ISBN})");
        if (book == null) return;

        var copies = await _bookService.GetCopiesByIsbnAsync(book.ISBN);
        if (copies.Count == 0)
        {
            Console.WriteLine("No copies found for this book.");
            return;
        }

        Console.WriteLine($"\n{"Copy ID",-10} {"Status",-25}");
        Console.WriteLine(new string('-', 35));
        foreach (var c in copies)
        {
            Console.WriteLine($"{c.CopyId,-10} {c.Status,-25}");
        }
    }

    private async Task MarkCopyStatusAsync()
    {
        Console.WriteLine("\n--- Mark Copy Status ---");
        var books = await _bookService.GetAllBooksAsync();
        if (books.Count == 0)
        {
            Console.WriteLine("No books found in the library.");
            return;
        }

        var book = InputValidator.GetSelection(books, "Select Book: ", b => $"{b.Title} (ISBN: {b.ISBN})");
        if (book == null) return;

        var copies = await _bookService.GetCopiesByIsbnAsync(book.ISBN);
        if (copies.Count == 0)
        {
            Console.WriteLine("No copies found for this book.");
            return;
        }

        var copy = InputValidator.GetSelection(copies, "Select Copy: ", c => $"Copy #{c.CopyId} - Current Status: {c.Status}");
        if (copy == null) return;

        Console.WriteLine("\nNew Status Options:");
        var statusOptions = new List<string> { "Available", "MinorDamage", "DamagedBeyondUsable", "Lost" };
        var selectedStatus = InputValidator.GetSelection(statusOptions, "Select new status: ", s => s);
        if (selectedStatus == null) return;

        var result = await _bookService.MarkCopyStatusAsync(copy.CopyId, selectedStatus);
        Console.WriteLine(result);
    }
}
