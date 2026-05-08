using BusBookingSystem.DTOs;

namespace BusBookingSystem.Interfaces
{
    /// <summary>
    /// Defines the contract (abstraction) for authentication operations.
    ///
    /// OOP Principle — Abstraction:
    ///   Separating the interface from its implementation (AuthService) allows
    ///   consumers to depend on the contract, not the concrete class.
    ///   This enables easy mocking for unit tests and alternative implementations.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterCustomerAsync(RegisterDto dto);
        Task<AuthResponseDto> RegisterOperatorAsync(OperatorRegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
