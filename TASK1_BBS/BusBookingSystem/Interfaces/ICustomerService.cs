using BusBookingSystem.DTOs;

namespace BusBookingSystem.Interfaces
{
    /// <summary>
    /// Defines the contract (abstraction) for all customer-facing booking operations.
    ///
    /// OOP Principle — Abstraction:
    ///   Customers interact with the system through this interface contract.
    ///   The CustomerService implementation can be swapped, decorated (e.g., with caching),
    ///   or mocked without changing any controller code.
    /// </summary>
    public interface ICustomerService
    {
        Task<List<BusSearchResultDto>> SearchBusesAsync(string source, string destination, DateOnly date);
        Task<SeatBlockResponseDto> BlockSeatsAsync(int userId, SeatBlockRequestDto dto);
        Task<BookingResponseDto> CreateBookingAsync(int userId, CreateBookingDto dto);
        Task<PaymentResponseDto> ProcessPaymentAsync(int userId, PaymentRequestDto dto);
        Task<List<BookingListDto>> GetMyBookingsAsync(int userId);
        Task<BookingResponseDto> GetBookingDetailAsync(int userId, int bookingId);
        Task<CancelResponseDto> CancelBookingAsync(int userId, int bookingId, CancelBookingDto dto);
        Task<ProfileDto> GetProfileAsync(int userId);
        Task<ProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<BusSearchResultDto> GetScheduleDetailsAsync(int scheduleId);
    }
}
