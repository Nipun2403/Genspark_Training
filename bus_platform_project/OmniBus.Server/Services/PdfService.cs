using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OmniBus.Server.Models;
using QRCoder;

namespace OmniBus.Server.Services
{
    public interface IPdfService
    {
        byte[] GenerateTicketPdf(Booking booking);
    }

    public class PdfService : IPdfService
    {
        public byte[] GenerateTicketPdf(Booking booking)
        {
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A5, 30, 30, 30, 30);
            var writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // Header
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, new BaseColor(187, 134, 252));
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.DarkGray);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.Black);

            doc.Add(new Paragraph("🚌 OmniBus E-Ticket", headerFont) { SpacingAfter = 15 });
            doc.Add(new Paragraph($"Booking ID: {booking.BookingId}", normalFont));
            doc.Add(new Paragraph($"Payment Ref: {booking.PaymentRef ?? "N/A"}", normalFont));
            doc.Add(new Paragraph($"Status: {booking.Status}", boldFont) { SpacingAfter = 10 });

            // Route info
            doc.Add(new Paragraph($"Route: {booking.Bus.Route.SourceCity} → {booking.Bus.Route.DestinationCity}", boldFont));
            doc.Add(new Paragraph($"Bus: {booking.Bus.BusNumber} ({booking.Bus.PlateNumber})", normalFont));
            doc.Add(new Paragraph($"Departure: {booking.Bus.DepartureTime:dd MMM yyyy, hh:mm tt}", normalFont));
            doc.Add(new Paragraph($"Pickup: {booking.Bus.PickupAddress}", normalFont));
            doc.Add(new Paragraph($"Drop: {booking.Bus.DropoffAddress}", normalFont) { SpacingAfter = 10 });

            // Passengers table
            var table = new PdfPTable(4) { WidthPercentage = 100, SpacingBefore = 10 };
            table.SetWidths(new float[] { 1, 3, 1, 2 });
            foreach (var h in new[] { "Seat", "Name", "Age", "Mobile" })
                table.AddCell(new PdfPCell(new Phrase(h, boldFont)) { BackgroundColor = new BaseColor(30, 30, 30), Padding = 6 });

            foreach (var bs in booking.BookingSeats)
            {
                table.AddCell(new PdfPCell(new Phrase(bs.Seat.SeatNumber.ToString(), normalFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(bs.PassengerName, normalFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(bs.PassengerAge.ToString(), normalFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(bs.PassengerMobile, normalFont)) { Padding = 5 });
            }
            doc.Add(table);

            // Total
            doc.Add(new Paragraph($"\nTotal Amount: ₹{booking.TotalAmount:N2}", headerFont) { SpacingBefore = 10 });
            if (booking.DiscountPercent > 0)
                doc.Add(new Paragraph($"Discount Applied: {booking.DiscountPercent}%", normalFont));

            // QR Code
            var qrData = $"OMNIBUS|{booking.BookingId}|{booking.PaymentRef}|{booking.Bus.BusNumber}";
            using var qrGen = new QRCodeGenerator();
            using var qrCodeData = qrGen.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(5);
            var qrImage = Image.GetInstance(qrBytes);
            qrImage.ScaleAbsolute(100, 100);
            qrImage.Alignment = Element.ALIGN_CENTER;
            doc.Add(qrImage);

            doc.Close();
            return ms.ToArray();
        }
    }
}
