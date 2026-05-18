namespace DataAccessLayer.Entities;

public class BookCategory
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // one category has many books
    public List<Book> Books { get; set; } = new();
}
