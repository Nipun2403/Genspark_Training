using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(b => b.Copies)
            .OrderBy(b => b.ISBN)
            .ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        var allBooks = await GetAllAsync();
        return allBooks.ElementAtOrDefault(id - 1);
    }

    public async Task AddAsync(Book book, int availableCopies)
    {
        var defaultCategory = await _context.BookCategories.FindAsync(1);
        if (defaultCategory == null)
        {
            defaultCategory = new BookCategory { CategoryId = 1, CategoryName = "General" };
            _context.BookCategories.Add(defaultCategory);
            await _context.SaveChangesAsync();
        }

        book.CategoryId = 1;
        _context.Books.Add(book);

        for (int i = 0; i < availableCopies; i++)
        {
            _context.BookCopies.Add(new BookCopy
            {
                ISBN = book.ISBN,
                Status = "Available"
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<Book>> SearchByTitleAsync(string title)
    {
        var lowerTitle = title.ToLower();
        return await _context.Books
            .Include(b => b.Copies)
            .Where(b => b.Title.ToLower().Contains(lowerTitle))
            .OrderBy(b => b.ISBN)
            .ToListAsync();
    }
}
