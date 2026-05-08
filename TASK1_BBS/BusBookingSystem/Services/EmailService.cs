using BusBookingSystem.DTOs;
using BusBookingSystem.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BusBookingSystem.Services
{
    // OOP — Abstraction: Interface moved to Interfaces/IEmailService.cs.
    // DTO classes (BookingEmailDto, PassengerInfo, CancellationEmailDto)
    // moved to DTOs/Dtos.cs (Encapsulation — each layer owns its own types).

    /// <summary>
    /// Concrete email service using MailKit/SMTP.
    /// Implements IEmailService (defined in Interfaces/).
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _config["Smtp:FromName"] ?? "BusBooking Platform",
                    _config["Smtp:Username"] ?? "dharshini.k2022cce@sece.ac.in"));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _config["Smtp:Host"] ?? "smtp.gmail.com",
                    int.Parse(_config["Smtp:Port"] ?? "587"),
                    SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    _config["Smtp:Username"],
                    _config["Smtp:Password"]);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                // Log but don't throw — email failure should not break the booking flow
                _logger.LogWarning("Failed to send email to {Email}: {Error}", toEmail, ex.Message);
            }
        }

        public async Task SendBookingConfirmationAsync(string toEmail, string name, BookingEmailDto booking)
        {
            var seats = string.Join(", ", booking.SeatNumbers);
            var passengerRows = string.Concat(booking.Passengers.Select((p, i) =>
            {
                var bg = i % 2 == 0 ? "#f8f8ff" : "#fff";
                return $"<tr style=\"background:{bg}\">"
                     + $"<td style=\"padding:8px 12px;color:#333;font-size:13px;\">{p.SeatNumber}</td>"
                     + $"<td style=\"padding:8px 12px;color:#333;font-size:13px;\">{p.Name}</td>"
                     + $"<td style=\"padding:8px 12px;color:#333;font-size:13px;\">{p.Age} yrs</td>"
                     + $"<td style=\"padding:8px 12px;color:#333;font-size:13px;\">{p.Gender}</td>"
                     + "</tr>";
            }));

            var html = $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family: 'Segoe UI', Arial, sans-serif; background: #eef2f7; padding: 24px; margin: 0;">
                  <div style="max-width: 600px; margin: auto; background: #fff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 32px rgba(0,0,0,.12);">

                    <!-- Header -->
                    <div style="background: linear-gradient(135deg,#667eea 0%,#764ba2 100%); padding: 36px 32px; text-align: center;">
                      <div style="font-size: 48px; margin-bottom: 8px;">🎉</div>
                      <h1 style="color:#fff; margin:0; font-size:28px; font-weight:700;">Booking Confirmed!</h1>
                      <p style="color:rgba(255,255,255,.85); margin:8px 0 0; font-size:15px;">Your ticket is ready — enjoy your journey!</p>
                    </div>

                    <!-- Ticket body -->
                    <div style="padding: 28px 32px;">
                      <p style="color:#333; font-size:16px; margin-top:0;">Hi <strong>{name}</strong>, here are your journey details:</p>

                      <!-- Route banner -->
                      <div style="background: linear-gradient(135deg,#f5f7fa,#e8ecf0); border-radius:12px; padding:20px 24px; margin:20px 0; display:flex; align-items:center; justify-content:space-between;">
                        <div style="text-align:center; flex:1;">
                          <div style="font-size:22px; font-weight:800; color:#333;">{booking.Source}</div>
                          <div style="font-size:13px; color:#888; margin-top:4px;">{booking.DepartureTime:HH:mm}</div>
                          <div style="font-size:12px; color:#aaa;">{booking.DepartureTime:ddd dd MMM}</div>
                          <div style="font-size:11px; color:#667eea; margin-top:8px; font-style:italic;">Boarding: {booking.BoardingAddress}</div>
                        </div>
                        <div style="text-align:center; width:100px; padding:0 10px;">
                          <div style="color:#667eea; font-size:20px;">🚌</div>
                          <div style="height:2px; background:linear-gradient(90deg,#667eea,#764ba2); border-radius:2px; margin:6px 0;"></div>
                          <div style="font-size:10px; color:#888; white-space:nowrap;">{booking.BusName}</div>
                        </div>
                        <div style="text-align:center; flex:1;">
                          <div style="font-size:22px; font-weight:800; color:#333;">{booking.Destination}</div>
                          <div style="font-size:13px; color:#888; margin-top:4px;">{booking.ArrivalTime:HH:mm}</div>
                          <div style="font-size:12px; color:#aaa;">{booking.ArrivalTime:ddd dd MMM}</div>
                          <div style="font-size:11px; color:#667eea; margin-top:8px; font-style:italic;">Drop: {booking.DroppingAddress}</div>
                        </div>
                      </div>

                      <!-- Booking meta -->
                      <table style="width:100%; border-collapse:collapse; margin-bottom:20px;">
                        <tr style="background:#f8f8ff;">
                          <td style="padding:10px 14px; color:#888; font-size:13px;">Booking ID</td>
                          <td style="padding:10px 14px; font-weight:bold; color:#667eea; font-size:16px;">#BBS{booking.BookingId:D6}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 14px; color:#888; font-size:13px;">Seats</td>
                          <td style="padding:10px 14px; color:#333; font-weight:600;">{seats}</td>
                        </tr>
                        <tr style="background:#f8f8ff;">
                          <td style="padding:10px 14px; color:#888; font-size:13px;">Base Fare</td>
                          <td style="padding:10px 14px; color:#333;">₹{booking.BaseAmount:F2}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 14px; color:#888; font-size:13px;">Platform Fee</td>
                          <td style="padding:10px 14px; color:#333;">₹{booking.PlatformFee:F2}</td>
                        </tr>
                        <tr style="background:#667eea;">
                          <td style="padding:12px 14px; color:#fff; font-weight:700; font-size:15px;">Total Paid</td>
                          <td style="padding:12px 14px; color:#fff; font-weight:800; font-size:18px;">₹{booking.GrandTotal:F2}</td>
                        </tr>
                      </table>

                      <!-- Passengers -->
                      <h3 style="font-size:14px; color:#555; text-transform:uppercase; letter-spacing:.06em; margin-bottom:8px;">Passengers</h3>
                      <table style="width:100%; border-collapse:collapse; border-radius:8px; overflow:hidden;">
                        <thead>
                          <tr style="background:#667eea;">
                            <th style="padding:8px 12px; color:#fff; font-size:12px; text-align:left;">Seat</th>
                            <th style="padding:8px 12px; color:#fff; font-size:12px; text-align:left;">Name</th>
                            <th style="padding:8px 12px; color:#fff; font-size:12px; text-align:left;">Age</th>
                            <th style="padding:8px 12px; color:#fff; font-size:12px; text-align:left;">Gender</th>
                          </tr>
                        </thead>
                        <tbody>{passengerRows}</tbody>
                      </table>

                      <!-- Advisory -->
                      <div style="background:#e8f5e9; border-left:4px solid #4caf50; padding:14px 18px; border-radius:4px; margin-top:24px;">
                        <p style="margin:0; color:#2e7d32; font-size:14px;">✅ Please arrive at the boarding point <strong>15 minutes before departure</strong>. Carry a valid photo ID.</p>
                      </div>
                    </div>

                    <!-- Footer -->
                    <div style="background:#f8f8ff; padding:16px 32px; text-align:center; border-top:1px solid #eee;">
                      <p style="color:#aaa; font-size:12px; margin:0;">BusBooking Platform &bull; This is a system-generated ticket. No signature required.</p>
                    </div>
                  </div>
                </body>
                </html>
                """;

            await SendAsync(toEmail, name, $"🎉 Booking Confirmed #BBS{booking.BookingId:D6} | {booking.Source} → {booking.Destination} on {booking.DepartureTime:dd MMM yyyy}", html);
        }

        public async Task SendBookingCancellationAsync(string toEmail, string name, CancellationEmailDto cancellation)
        {
            var html = $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;">
                  <div style="max-width: 560px; margin: auto; background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,.08);">
                    <div style="background: linear-gradient(135deg,#f093fb,#f5576c); padding: 32px; text-align: center;">
                      <h1 style="color:#fff; margin:0; font-size:26px;">Booking Cancelled</h1>
                    </div>
                    <div style="padding: 28px;">
                      <p style="color:#333; font-size:16px;">Hi <strong>{name}</strong>,</p>
                      <p style="color:#555;">Your booking has been cancelled. Here's a summary:</p>

                      <table style="width:100%; border-collapse:collapse; margin:20px 0;">
                        <tr style="background:#f8f8ff;">
                          <td style="padding:10px 14px; color:#888; font-size:13px;">Booking ID</td>
                          <td style="padding:10px 14px; font-weight:bold; color:#333;">#{cancellation.BookingId}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 14px; color:#888; font-size:13px;">Bus</td>
                          <td style="padding:10px 14px; color:#333;">{cancellation.BusName}</td>
                        </tr>
                        <tr style="background:#f8f8ff;">
                          <td style="padding:10px 14px; color:#888; font-size:13px;">Route</td>
                          <td style="padding:10px 14px; color:#333;">{cancellation.Source} → {cancellation.Destination}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 14px; color:#888; font-size:13px;">Refund Amount</td>
                          <td style="padding:10px 14px; font-weight:bold; color:#f5576c; font-size:18px;">₹{cancellation.RefundAmount:F2}</td>
                        </tr>
                        <tr style="background:#f8f8ff;">
                          <td style="padding:10px 14px; color:#888; font-size:13px;">Policy Applied</td>
                          <td style="padding:10px 14px; color:#555; font-size:13px;">{cancellation.RefundPolicy}</td>
                        </tr>
                      </table>

                      {(cancellation.RefundAmount > 0
                        ? "<div style=\"background:#fff3e0; border-left:4px solid #ff9800; padding:14px 18px; border-radius:4px;\"><p style=\"margin:0; color:#e65100; font-size:14px;\">💳 Refund will be credited to your original payment method within 5–7 business days.</p></div>"
                        : "<div style=\"background:#fce4ec; border-left:4px solid #e91e63; padding:14px 18px; border-radius:4px;\"><p style=\"margin:0; color:#880e4f; font-size:14px;\">❌ No refund applicable as per our cancellation policy.</p></div>")}
                    </div>
                    <div style="background:#f8f8ff; padding:16px; text-align:center;">
                      <p style="color:#aaa; font-size:12px; margin:0;">BusBooking Platform • Cancellation notification.</p>
                    </div>
                  </div>
                </body>
                </html>
                """;

            await SendAsync(toEmail, name, $"Booking Cancelled — #{cancellation.BookingId} | Refund: ₹{cancellation.RefundAmount:F2}", html);
        }

        public async Task SendBusServiceCancellationAsync(string toEmail, string name, string busName, string source, string destination, DateTime departureTime)
        {
            var html = $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;">
                  <div style="max-width: 560px; margin: auto; background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,.08);">
                    <div style="background: #e53e3e; padding: 32px; text-align: center;">
                      <h1 style="color:#fff; margin:0; font-size:26px;">Service Cancelled 🚌💨</h1>
                    </div>
                    <div style="padding: 28px;">
                      <p style="color:#333; font-size:16px;">Hi <strong>{name}</strong>,</p>
                      <p style="color:#555;">We regret to inform you that the bus service you booked has been cancelled by the operator.</p>

                      <div style="background: #fff5f5; border: 1px solid #feb2b2; border-radius: 8px; padding: 20px; margin: 20px 0;">
                        <div style="font-weight: bold; color: #c53030; margin-bottom: 8px;">Journey Details:</div>
                        <div style="color: #4a5568; font-size: 15px;">
                          <strong>Bus:</strong> {busName}<br>
                          <strong>Route:</strong> {source} → {destination}<br>
                          <strong>Date:</strong> {departureTime:ddd dd MMM yyyy}<br>
                          <strong>Time:</strong> {departureTime:HH:mm}
                        </div>
                      </div>

                      <div style="background:#f0fff4; border-left:4px solid #48bb78; padding:14px 18px; border-radius:4px;">
                        <p style="margin:0; color:#2f855a; font-size:14px;">💳 <strong>Full Refund Initiated:</strong> Since this cancellation was from our side, a 100% refund has been initiated to your original payment method. It will reflect in your account within 5–7 business days.</p>
                      </div>

                      <p style="color: #718096; font-size: 13px; margin-top: 24px;">We apologize for the inconvenience caused. You can book another bus for your journey on our platform.</p>
                    </div>
                    <div style="background:#f8f8ff; padding:16px; text-align:center;">
                      <p style="color:#aaa; font-size:12px; margin:0;">BusBooking Platform • Important Service Update</p>
                    </div>
                  </div>
                </body>
                </html>
                """;

            await SendAsync(toEmail, name, $"⚠️ URGENT: Bus Service Cancelled — {source} to {destination}", html);
        }

        public async Task SendOperatorApprovalAsync(string toEmail, string companyName, bool approved)
        {
            var status = approved ? "Approved ✅" : "Rejected ❌";
            var color = approved ? "#667eea" : "#f5576c";
            var message = approved
                ? "Congratulations! Your operator account has been approved. You can now log in and start adding buses and schedules."
                : "Unfortunately, your operator registration has been rejected by the admin. Please contact support for more information.";

            var html = $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;">
                  <div style="max-width: 560px; margin: auto; background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,.08);">
                    <div style="background: {color}; padding: 32px; text-align: center;">
                      <h1 style="color:#fff; margin:0; font-size:24px;">Operator Account {status}</h1>
                    </div>
                    <div style="padding: 28px;">
                      <p style="color:#333; font-size:16px;">Hi <strong>{companyName}</strong>,</p>
                      <p style="color:#555; font-size:15px;">{message}</p>
                    </div>
                    <div style="background:#f8f8ff; padding:16px; text-align:center;">
                      <p style="color:#aaa; font-size:12px; margin:0;">BusBooking Platform</p>
                    </div>
                  </div>
                </body>
                </html>
                """;

            await SendAsync(toEmail, companyName, $"Operator Account {status} — BusBooking Platform", html);
        }
    }
}
