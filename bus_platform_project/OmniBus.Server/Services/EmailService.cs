using MailKit.Net.Smtp;
using MimeKit;

namespace OmniBus.Server.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);
        Task SendTicketEmailAsync(string toEmail, string passengerName, byte[] pdfTicket);
        Task SendCancellationEmailAsync(string toEmail, string busDetails, string refundStatus, string? couponCode, string alternativeBusesLink);
        Task SendOperatorStatusEmailAsync(string toEmail, string status, string? reason);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var subject = "OmniBus - Your Login OTP";
            var body = $@"
            <div style='font-family: Inter, sans-serif; background: #121212; color: #E0E0E0; padding: 40px; border-radius: 12px;'>
                <h2 style='color: #BB86FC;'>🚌 OmniBus Login</h2>
                <p>Your one-time verification code is:</p>
                <div style='background: #1E1E1E; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                    <span style='font-size: 32px; letter-spacing: 8px; color: #BB86FC; font-weight: bold;'>{otpCode}</span>
                </div>
                <p style='color: #888;'>This code expires in 10 minutes. Do not share it with anyone.</p>
            </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendTicketEmailAsync(string toEmail, string passengerName, byte[] pdfTicket)
        {
            var subject = "OmniBus - Your Ticket Confirmation 🎫";
            var body = $@"
            <div style='font-family: Inter, sans-serif; background: #121212; color: #E0E0E0; padding: 40px; border-radius: 12px;'>
                <h2 style='color: #BB86FC;'>🎉 Booking Confirmed!</h2>
                <p>Hi {passengerName}, your bus ticket has been confirmed.</p>
                <p>Please find your ticket attached as a PDF with QR code.</p>
                <p style='color: #888;'>Have a safe journey! — Team OmniBus</p>
            </div>";

            var message = CreateMessage(toEmail, subject, body);
            var attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(new MemoryStream(pdfTicket)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "OmniBus_Ticket.pdf"
            };

            var multipart = new Multipart("mixed");
            multipart.Add(message.Body);
            multipart.Add(attachment);
            message.Body = multipart;

            await SendMimeMessageAsync(message);
        }

        public async Task SendCancellationEmailAsync(string toEmail, string busDetails, string refundStatus, string? couponCode, string alternativeBusesLink)
        {
            var couponSection = couponCode != null
                ? $"<p>As a gesture of apology, here's a <strong>10% discount code</strong>: <span style='color: #BB86FC; font-weight: bold;'>{couponCode}</span></p>"
                : "";

            var subject = "OmniBus - Important: Your Bus Has Been Cancelled";
            var body = $@"
            <div style='font-family: Inter, sans-serif; background: #121212; color: #E0E0E0; padding: 40px; border-radius: 12px;'>
                <h2 style='color: #CF6679;'>⚠️ Bus Service Cancelled</h2>
                <p>We regret to inform you that the following service has been cancelled:</p>
                <div style='background: #1E1E1E; padding: 16px; border-radius: 8px; margin: 16px 0;'>{busDetails}</div>
                <p><strong>Refund Status:</strong> {refundStatus}</p>
                {couponSection}
                <p><a href='{alternativeBusesLink}' style='color: #BB86FC;'>View Alternative Buses →</a></p>
            </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendOperatorStatusEmailAsync(string toEmail, string status, string? reason)
        {
            var subject = $"OmniBus - Your Operator Account has been {status}";
            var body = $@"
            <div style='font-family: Inter, sans-serif; background: #121212; color: #E0E0E0; padding: 40px; border-radius: 12px;'>
                <h2 style='color: #BB86FC;'>Operator Account Update</h2>
                <p>Your operator account status: <strong style='color: {(status == "Approved" ? "#4CAF50" : "#CF6679")};'>{status}</strong></p>
                {(reason != null ? $"<p>Reason: {reason}</p>" : "")}
            </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private MimeMessage CreateMessage(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:SenderName"] ?? "OmniBus",
                _config["Email:SenderEmail"] ?? "noreply@omnibus.com"));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };
            return message;
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = CreateMessage(toEmail, subject, htmlBody);
            await SendMimeMessageAsync(message);
        }

        private async Task SendMimeMessageAsync(MimeMessage message)
        {
            try
            {
                using var smtp = new SmtpClient();
                var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
                var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
                var user = _config["Email:SmtpUser"] ?? "";
                var pass = _config["Email:SmtpPass"] ?? "";

                var security = port == 465 ? MailKit.Security.SecureSocketOptions.SslOnConnect : MailKit.Security.SecureSocketOptions.StartTls;
                await smtp.ConnectAsync(host, port, security);
                if (!string.IsNullOrEmpty(user))
                    await smtp.AuthenticateAsync(user, pass);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {To}: {Subject}", message.To, message.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}: {Subject}", message.To, message.Subject);
                // Don't throw — email failures shouldn't break business logic
            }
        }
    }
}
