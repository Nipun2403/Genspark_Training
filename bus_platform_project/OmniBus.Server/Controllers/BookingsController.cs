using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Services;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly OmniBusDbContext _db;
        private readonly IPdfService _pdfService;

        public BookingsController(IBookingService bookingService, OmniBusDbContext db, IPdfService pdfService)
        {
            _bookingService = bookingService;
            _db = db;
            _pdfService = pdfService;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request) =>
            Ok(await _bookingService.CreateBookingAsync(request, UserId));

        [HttpGet("my")]
        public async Task<IActionResult> GetMy() =>
            Ok(await _bookingService.GetUserBookingsAsync(UserId));

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> Get(Guid bookingId) =>
            Ok(await _bookingService.GetBookingAsync(bookingId, UserId));

        [HttpPost("{bookingId}/cancel")]
        public async Task<IActionResult> Cancel(Guid bookingId) =>
            Ok(await _bookingService.CancelBookingAsync(bookingId, UserId));

        [HttpGet("{bookingId}/ticket")]
        public async Task<IActionResult> DownloadTicket(Guid bookingId)
        {
            var booking = await _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Bus).ThenInclude(b => b.Route)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == UserId);

            if (booking == null) return NotFound("Booking not found");

            var pdf = _pdfService.GenerateTicketPdf(booking);
            return File(pdf, "application/pdf", $"OmniBus_Ticket_{bookingId:N}.pdf");
        }
    }
}
