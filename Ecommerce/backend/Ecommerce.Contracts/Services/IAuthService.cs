using System.Threading.Tasks;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IAuthService
    {
        Task<RegisterResponse> Register(RegisterRequest request);
        Task<TokenResponse> Login(LoginRequest request);
        Task<TokenResponse> RefreshToken(string expiredAccessToken, string refreshToken);
        Task<bool> Logout(string refreshToken, int userId);
        Task<bool> RevokeAllTokens(int userId);
        Task<bool> SendForgotPasswordOtp(string email);
        Task<bool> VerifyForgotPasswordOtp(string email, string otp);
        Task<bool> ResetPasswordWithOtp(string email, string otp, string newPassword);
    }
}