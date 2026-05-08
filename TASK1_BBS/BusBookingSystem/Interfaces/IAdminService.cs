using BusBookingSystem.DTOs;

namespace BusBookingSystem.Interfaces
{
    /// <summary>
    /// Defines the contract (abstraction) for all admin operations.
    ///
    /// OOP Principle — Abstraction:
    ///   AdminController depends on this interface, not on AdminService directly.
    ///   This decouples the controller from the implementation and enables
    ///   dependency injection, testability, and future swapping of implementations.
    /// </summary>
    public interface IAdminService
    {
        Task<List<OperatorListDto>> GetOperatorsAsync(string? status);
        Task<MessageResponseDto> ApproveOperatorAsync(int adminId, int operatorId);
        Task<MessageResponseDto> DisableOperatorAsync(int adminId, int operatorId);
        Task<List<BusPendingDto>> GetBusesAsync(string? status);
        Task<MessageResponseDto> ApproveBusAsync(int adminId, int busId);
        Task<MessageResponseDto> DisableBusAsync(int adminId, int busId);
        Task<List<RouteDto>> GetRoutesAsync();
        Task<RouteDto> CreateRouteAsync(int adminId, CreateRouteDto dto);
        Task<MessageResponseDto> ToggleRouteAsync(int routeId);
        Task<RevenueDto> GetRevenueAsync();
        Task<List<PlatformSettingDto>> GetSettingsAsync();
        Task<MessageResponseDto> UpdateSettingAsync(int adminId, string key, string value);
        Task<MessageResponseDto> CancelScheduleAsync(int adminId, int scheduleId);
        Task<CancelResponseDto> CancelBookingAsync(int adminId, int bookingId, CancelBookingDto dto);
    }
}
