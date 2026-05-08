using BusBookingSystem.DTOs;

namespace BusBookingSystem.Interfaces
{
    /// <summary>
    /// Defines the contract (abstraction) for all email notification operations.
    ///
    /// OOP Principle — Abstraction:
    ///   Services depend on IEmailService — not on EmailService directly.
    ///   This allows the email provider to be swapped (SendGrid, MailKit, mock)
    ///   without changing any service that sends emails.
    /// </summary>
    public interface IEmailService
    {
        Task SendBookingConfirmationAsync(string toEmail, string name, BookingEmailDto booking);
        Task SendBookingCancellationAsync(string toEmail, string name, CancellationEmailDto cancellation);
        Task SendBusServiceCancellationAsync(string toEmail, string name, string busName, string source, string destination, DateTime departureTime);
        Task SendOperatorApprovalAsync(string toEmail, string companyName, bool approved);
    }
}
