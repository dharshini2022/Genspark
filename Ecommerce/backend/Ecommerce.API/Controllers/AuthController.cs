using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.Register(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.Login(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return Unauthorized(new { message = "Refresh token is missing or expired." });
            }

            if (string.IsNullOrEmpty(request.ExpiredAccessToken))
            {
                return Unauthorized(new { message = "Access token is missing." });
            }

            try
            {
                var result = await _authService.RefreshToken(request.ExpiredAccessToken, request.RefreshToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var result = await _authService.Logout(request.RefreshToken, request.UserId);
            return Ok(new { success = result });
        }


        [AllowAnonymous]
        [HttpPost("forgot-password/send-otp")]
        public async Task<IActionResult> SendForgotPasswordOtp([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var success = await _authService.SendForgotPasswordOtp(request.Email);
                return Ok(new { success, message = "OTP sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/verify-otp")]
        public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] ForgotPasswordVerifyOtpRequest request)
        {
            var isValid = await _authService.VerifyForgotPasswordOtp(request.Email, request.Otp);
            if (!isValid)
            {
                return BadRequest(new { message = "Invalid or expired OTP." });
            }
            return Ok(new { success = true, message = "OTP verified successfully." });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/reset-password")]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ForgotPasswordResetPasswordRequest request)
        {
            try
            {
                var success = await _authService.ResetPasswordWithOtp(request.Email, request.Otp, request.NewPassword);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to reset password." });
                }
                return Ok(new { success = true, message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
