using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories;

public interface IBookRepository
{
    Task<List<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task AddAsync(Book book, int availableCopies);
    Task<List<Book>> SearchByTitleAsync(string title);
}
