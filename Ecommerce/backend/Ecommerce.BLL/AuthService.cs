using System;
using System.Text;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; 
using System.Threading.Tasks;         
using Microsoft.IdentityModel.Tokens; 
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Ecommerce.Contracts.Services;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models.DTOs;
using Ecommerce.Models;
using Ecommerce.DAL.Context;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Ecommerce.BLL
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private ICurrentUserService _currentUserService;

        private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _forgotPasswordOtps = new();

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IMapper mapper,  IConfiguration configuration, ICurrentUserService currentUserService, IEmailService emailService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
            _configuration = configuration;
            _currentUserService = currentUserService;
            _emailService = emailService;
        }

        public async Task<RegisterResponse> Register(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmail(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists with this email");
            }

            var user = _mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.IsActive = true;

            if(_currentUserService.Role == "Admin"){
                user.Role = UserRole.Admin;
            }
            else{
                user.Role = UserRole.Customer;
            }
            
            var createdUser = await _userRepository.Create(user);

            return  _mapper.Map<RegisterResponse>(createdUser);
        }

        public async Task<TokenResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.GetByEmail(request.Email);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials or account is suspended / Deactivated");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var accessToken = GenerateJwtToken(user);
            var refreshTokenString = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            var hashedRefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshTokenString);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = hashedRefreshToken,
                ExpiresAt = DateTime.Now.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenRepository.Create(refreshTokenEntity);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                ExpiresAt = DateTime.Now.AddMinutes(30),
                Role = user.Role.ToString()
            };
        }

        public async Task<TokenResponse> RefreshToken(string expiredAccessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(expiredAccessToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid token claims.");
            }

            var tokenEntities = await _refreshTokenRepository.GetTokensByUserIdWithUser(userId);
            
            // Optimize BCrypt verify loop by only verifying active (unrevoked, unexpired) tokens.
            var activeTokenEntities = tokenEntities.Where(rt => !rt.IsRevoked && rt.ExpiresAt > DateTime.Now);
            var tokenEntity = activeTokenEntities.FirstOrDefault(rt => BCrypt.Net.BCrypt.Verify(refreshToken, rt.Token));
            
            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt < DateTime.Now)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            tokenEntity.IsRevoked = true;
            await _refreshTokenRepository.Update(tokenEntity.Id, tokenEntity);

            var newRefreshTokenString = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = tokenEntity.UserId,
                Token = BCrypt.Net.BCrypt.HashPassword(newRefreshTokenString),
                ExpiresAt = DateTime.Now.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenRepository.Create(newRefreshTokenEntity);

            var newAccessToken = GenerateJwtToken(tokenEntity.User);

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                ExpiresAt = newRefreshTokenEntity.ExpiresAt,
                Role = tokenEntity.User.Role.ToString()
            };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "SuperSecretKeyForJWTEcommerceProject2026!")),
                ValidateLifetime = false, 
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "Ecommerce.API",
                ValidAudience = _configuration["Jwt:Audience"] ?? "Ecommerce.Client"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken)
            {
                throw new SecurityTokenException("Invalid token format.");
            }

            var algorithm = jwtSecurityToken.Header.Alg;
            if (!algorithm.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase) &&
                !algorithm.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token signature algorithm.");
            }

            return principal;
        }

        public async Task<bool> Logout(string refreshToken, int userId)
        {
            var tokenEntity = await _refreshTokenRepository.GetByTokenAndUserId(refreshToken, userId);

            if (tokenEntity != null)
            {
                tokenEntity.IsRevoked = true;
                await _refreshTokenRepository.Update(tokenEntity.Id, tokenEntity);
                return true;
            }

            return false;
        }


        public async Task<bool> RevokeAllTokens(int userId)
        {
            var tokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _refreshTokenRepository.SaveChangesAsync();
            return true;
        }

        private string GenerateJwtToken(User user)
        {   
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var keyString = _configuration["Jwt:Key"] ?? "SuperSecretKeyForJWTEcommerceProject2026!";
            var key = Encoding.UTF8.GetBytes(keyString);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("fullName", user.FullName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(30),
                Issuer = _configuration["Jwt:Issuer"] ?? "Ecommerce.API",
                Audience = _configuration["Jwt:Audience"] ?? "Ecommerce.Client",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> SendForgotPasswordOtp(string email)
        {
            var user = await _userRepository.GetByEmail(email);
            if (user == null)
            {
                throw new InvalidOperationException("Email address is not registered.");
            }

            var otp = Random.Shared.Next(100000, 999999).ToString();
            var expiry = DateTime.Now.AddMinutes(10);
            _forgotPasswordOtps[email.ToLower().Trim()] = (otp, expiry);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOtpEmail(email, otp);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending password reset OTP email: {ex.Message}");
                }
            });

            return true;
        }

        public Task<bool> VerifyForgotPasswordOtp(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                return Task.FromResult(false);
            }

            var key = email.ToLower().Trim();
            if (_forgotPasswordOtps.TryGetValue(key, out var storedInfo))
            {
                if (storedInfo.Expiry >= DateTime.Now && storedInfo.Otp == otp.Trim())
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public async Task<bool> ResetPasswordWithOtp(string email, string otp, string newPassword)
        {
            var key = email.ToLower().Trim();
            if (!_forgotPasswordOtps.TryGetValue(key, out var storedInfo) || storedInfo.Expiry < DateTime.Now || storedInfo.Otp != otp.Trim())
            {
                throw new InvalidOperationException("Invalid or expired OTP.");
            }

            var user = await _userRepository.GetByEmail(email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            var success = await _userRepository.ChangePassword(user.Id, newHash);

            if (success)
            {
                _forgotPasswordOtps.TryRemove(key, out _);
            }

            return success;
        }
    }
}
