using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services;

/// CURD for Books :  add book, add copies, search, mark damaged.
public class BookService
{
    private readonly LibraryDbContext _context;

    public BookService(LibraryDbContext context)
    {
        _context = context;
    }

    /// Adds a new book to the library by ISBN.
    public async Task<string> AddBookAsync(string isbn, string title, string author, int categoryId)
    {
        // Check if ISBN already exists
        var exists = await _context.Books.AnyAsync(b => b.ISBN == isbn);
        if (exists)
            return $"Error: A book with ISBN '{isbn}' already exists.";

        // Check if category exists
        var categoryExists = await _context.BookCategories.AnyAsync(c => c.CategoryId == categoryId);
        if (!categoryExists)
            return $"Error: Category with ID {categoryId} does not exist.";

        var book = new Book
        {
            ISBN = isbn,
            Title = title,
            Author = author,
            CategoryId = categoryId
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return $"Book '{title}' (ISBN: {isbn}) added successfully.";
    }

    /// Adds a category to the library (case-insensitively).
    public async Task<string> AddCategoryAsync(string categoryName)
    {
        var exists = await _context.BookCategories
            .AnyAsync(c => c.CategoryName.ToLower() == categoryName.ToLower());

        if (exists)
            return $"Error: Category '{categoryName}' already exists.";

        var category = new BookCategory { CategoryName = categoryName };
        _context.BookCategories.Add(category);
        await _context.SaveChangesAsync();

        return $"Category '{categoryName}' added with ID: {category.CategoryId}.";
    }

    /// Adds a specified number of copies for a book.
    public async Task<string> AddCopiesAsync(string isbn, int numberOfCopies)
    {
        var bookExists = await _context.Books.AnyAsync(b => b.ISBN == isbn);
        if (!bookExists)
            return $"Error: No book found with ISBN '{isbn}'.";

        if (numberOfCopies <= 0)
            return "Error: Number of copies must be at least 1.";

        for (int i = 0; i < numberOfCopies; i++)
        {
            _context.BookCopies.Add(new BookCopy { ISBN = isbn });
        }

        await _context.SaveChangesAsync();

        return $"{numberOfCopies} copy/copies added for ISBN '{isbn}'.";
    }

    /// Returns all books with available copies (status: Available or MinorDamage).
    public async Task<List<Book>> GetAvailableBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .Where(b => b.Copies.Any(c => c.Status == "Available" || c.Status == "MinorDamage"))
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    /// Searches books by title, author, or category name
    public async Task<List<Book>> SearchBooksAsync(string searchTerm)
    {
        var lowerSearch = searchTerm.ToLower();

        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .Where(b =>
                b.Title.ToLower().Contains(lowerSearch) ||
                b.Author.ToLower().Contains(lowerSearch) ||
                b.Category.CategoryName.ToLower().Contains(lowerSearch) ||
                b.ISBN.ToLower().Contains(lowerSearch))
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    /// Marks a book copy as damaged or unavailable.
    public async Task<string> MarkCopyStatusAsync(int copyId, string newStatus)
    {
        var validStatuses = new[] { "Available", "MinorDamage", "DamagedBeyondUsable", "Lost" };
        if (!validStatuses.Contains(newStatus))
            return $"Error: Invalid status. Valid values: {string.Join(", ", validStatuses)}";

        var copy = await _context.BookCopies
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.CopyId == copyId);

        if (copy == null)
            return $"Error: Book copy with ID {copyId} not found.";

        if (copy.Status == "Borrowed")
            return "Error: Cannot change status of a currently borrowed copy. Return it first.";

        copy.Status = newStatus;
        await _context.SaveChangesAsync();

        return $"Copy {copyId} ('{copy.Book.Title}') status updated to '{newStatus}'.";
    }

    /// Returns all categories.
    public async Task<List<BookCategory>> GetAllCategoriesAsync()
    {
        return await _context.BookCategories
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    /// Gets all copies for a specific book by ISBN.
    public async Task<List<BookCopy>> GetCopiesByIsbnAsync(string isbn)
    {
        return await _context.BookCopies
            .Where(c => c.ISBN == isbn)
            .OrderBy(c => c.CopyId)
            .ToListAsync();
    }

    /// Returns all books in the system with their copies.
    public async Task<List<Book>> GetAllBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .OrderBy(b => b.Title)
            .ToListAsync();
    }
}
