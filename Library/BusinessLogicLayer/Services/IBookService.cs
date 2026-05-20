using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Services;

public interface IBookService
{
    Task<string> AddBookAsync(string isbn, string title, string author, int categoryId);
    Task<string> AddCategoryAsync(string categoryName);
    Task<string> AddCopiesAsync(string isbn, int numberOfCopies);
    Task<List<Book>> GetAvailableBooksAsync();
    Task<List<Book>> SearchBooksAsync(string searchTerm);
    Task<string> MarkCopyStatusAsync(int copyId, string newStatus);
    Task<List<BookCategory>> GetAllCategoriesAsync();
    Task<List<BookCopy>> GetCopiesByIsbnAsync(string isbn);
    Task<List<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(int id);
    Task AddBookAsync(Book book, int availableCopies);
    Task<List<Book>> SearchBooksByTitleAsync(string title);
}
