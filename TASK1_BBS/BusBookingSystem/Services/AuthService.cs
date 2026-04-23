using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusBookingSystem.Data;
using BusBookingSystem.DTOs;
using BusBookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BusBookingSystem.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterCustomerAsync(RegisterDto dto);
        Task<AuthResponseDto> RegisterOperatorAsync(OperatorRegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterDto dto)
        {
            if (!dto.AcceptedTerms)
                throw new InvalidOperationException("You must accept the terms and conditions.");

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Customer
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return GenerateToken(user);
        }

        public async Task<AuthResponseDto> RegisterOperatorAsync(OperatorRegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Operator
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var operatorProfile = new BusOperator
            {
                UserId = user.Id,
                CompanyName = dto.CompanyName,
                GSTNumber = dto.GSTNumber,
                Status = ApprovalStatus.Pending
            };

            _db.BusOperators.Add(operatorProfile);
            await _db.SaveChangesAsync();

            foreach (var o in dto.Offices)
            {
                _db.OperatorOffices.Add(new OperatorOffice
                {
                    OperatorId = operatorProfile.Id,
                    District = o.District,
                    City = o.City,
                    Address = o.Address
                });
            }

            await _db.SaveChangesAsync();
            return GenerateToken(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account has been disabled.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            // Operator must be approved
            if (user.Role == UserRole.Operator)
            {
                var op = await _db.BusOperators.FirstOrDefaultAsync(o => o.UserId == user.Id);
                if (op != null && op.Status == ApprovalStatus.Pending)
                    throw new UnauthorizedAccessException("Your operator account is pending admin approval.");
                if (op != null && op.Status == ApprovalStatus.Disabled)
                    throw new UnauthorizedAccessException("Your operator account has been disabled.");
            }

            return GenerateToken(user);
        }

        private AuthResponseDto GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Role = user.Role.ToString(),
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }
    }
}
