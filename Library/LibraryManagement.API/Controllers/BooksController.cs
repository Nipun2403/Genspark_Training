using BusinessLogicLayer.Services;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpPost]
    public async Task<IActionResult> AddBook([FromBody] BookCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Book title should not be empty." });
        }
        if (string.IsNullOrWhiteSpace(dto.Author))
        {
            return BadRequest(new { message = "Author name should not be empty." });
        }
        if (dto.AvailableCopies < 0)
        {
            return BadRequest(new { message = "Available copies should be greater than or equal to 0." });
        }

        var book = new Book
        {
            ISBN = dto.ISBN,
            Title = dto.Title,
            Author = dto.Author,
            PublishedYear = dto.PublishedYear
        };

        await _bookService.AddBookAsync(book, dto.AvailableCopies);

        return Ok(new { message = "Book added successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        var result = books.Select((b, index) => new BookResponseDto
        {
            BookId = index + 1,
            Title = b.Title,
            Author = b.Author,
            ISBN = b.ISBN,
            PublishedYear = b.PublishedYear,
            AvailableCopies = b.Copies.Count(c => c.Status == "Available" || c.Status == "MinorDamage")
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBookById(int id)
    {
        var books = await _bookService.GetAllBooksAsync();
        if (id <= 0 || id > books.Count)
        {
            return NotFound(new { message = "Book not found" });
        }

        var b = books[id - 1];
        var response = new BookResponseDto
        {
            BookId = id,
            Title = b.Title,
            Author = b.Author,
            ISBN = b.ISBN,
            PublishedYear = b.PublishedYear,
            AvailableCopies = b.Copies.Count(c => c.Status == "Available" || c.Status == "MinorDamage")
        };

        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchBooks([FromQuery] string title)
    {
        var allBooks = await _bookService.GetAllBooksAsync();
        var matchedBooks = await _bookService.SearchBooksByTitleAsync(title);

        var result = matchedBooks.Select(b =>
        {
            var index = allBooks.FindIndex(ab => ab.ISBN == b.ISBN);
            return new BookResponseDto
            {
                BookId = index >= 0 ? index + 1 : 0,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                PublishedYear = b.PublishedYear,
                AvailableCopies = b.Copies.Count(c => c.Status == "Available" || c.Status == "MinorDamage")
            };
        }).ToList();

        return Ok(result);
    }
}

public class BookCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int AvailableCopies { get; set; }
}

public class BookResponseDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int AvailableCopies { get; set; }
}
