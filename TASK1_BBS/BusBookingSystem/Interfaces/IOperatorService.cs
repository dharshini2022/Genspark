using BusBookingSystem.DTOs;

namespace BusBookingSystem.Interfaces
{
    /// <summary>
    /// Defines the contract (abstraction) for all bus operator operations.
    ///
    /// OOP Principle — Abstraction:
    ///   OperatorController depends on this interface — not on OperatorService directly.
    ///   If a new "PremiumOperatorService" needs to be added in the future,
    ///   it can implement this interface without touching the controller.
    /// </summary>
    public interface IOperatorService
    {
        Task<List<BusDto>> GetMyBusesAsync(int userId);
        Task<BusDto> AddBusAsync(int userId, AddBusDto dto);
        Task<MessageResponseDto> BringDownBusAsync(int userId, int busId);
        Task<MessageResponseDto> BringUpBusAsync(int userId, int busId);
        Task<List<LayoutDto>> GetLayoutsAsync(int userId);
        Task<LayoutDto> UploadLayoutAsync(int userId, UploadLayoutDto dto);
        Task<List<ScheduleDto>> GetSchedulesAsync(int userId);
        Task<ScheduleDto> CreateScheduleAsync(int userId, CreateScheduleDto dto);
        Task<MessageResponseDto> UpdatePriceAsync(int userId, int scheduleId, UpdatePriceDto dto);
        Task<List<BookingListDto>> GetOperatorBookingsAsync(int userId);
        Task<CancelResponseDto> CancelBookingAsync(int userId, int bookingId, CancelBookingDto dto);
        Task<MessageResponseDto> CancelScheduleAsync(int userId, int scheduleId);
        Task<List<OfficeDto>> GetOfficesAsync(int userId);
        Task<MessageResponseDto> AddOfficeAsync(int userId, OfficeDto dto);
        Task<MessageResponseDto> UpdateOfficeAsync(int userId, int officeId, OfficeDto dto);
        Task<OperatorDetailedRevenueDto> GetDetailedRevenueAsync(int userId, DateTime? startDate, DateTime? endDate, int? busId);
        Task<List<SchedulePassengerDto>> GetSchedulePassengersAsync(int userId, int scheduleId);
        Task<OperatorListDto> GetMyProfileAsync(int userId);
    }
}
