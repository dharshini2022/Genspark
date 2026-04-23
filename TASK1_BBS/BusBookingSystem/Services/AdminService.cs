using BusBookingSystem.Data;
using BusBookingSystem.DTOs;
using BusBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Services
{
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
    }

    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;

        public AdminService(AppDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        public async Task<List<OperatorListDto>> GetOperatorsAsync(string? status)
        {
            var query = _db.BusOperators
                .Include(o => o.User)
                .Include(o => o.Offices)
                .Include(o => o.Buses)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApprovalStatus>(status, true, out var approvalStatus))
                query = query.Where(o => o.Status == approvalStatus);

            return await query.Select(o => new OperatorListDto
            {
                Id = o.Id,
                UserId = o.UserId,
                OperatorName = o.User.Name,
                CompanyName = o.CompanyName,
                Email = o.User.Email,
                Phone = o.User.Phone,
                GSTNumber = o.GSTNumber,
                Status = o.Status.ToString(),
                RegisteredAt = o.RegisteredAt,
                Offices = o.Offices.Select(of => new OfficeDto
                {
                    District = of.District,
                    City = of.City,
                    Address = of.Address
                }).ToList(),
                TotalBuses = o.Buses.Count
            }).ToListAsync();
        }

        public async Task<MessageResponseDto> ApproveOperatorAsync(int adminId, int operatorId)
        {
            var op = await _db.BusOperators.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == operatorId)
                ?? throw new InvalidOperationException("Operator not found.");

            op.Status = ApprovalStatus.Approved;
            op.ApprovedByAdminId = adminId;
            op.User.IsActive = true;
            await _db.SaveChangesAsync();
            _ = _email.SendOperatorApprovalAsync(op.User.Email, op.CompanyName, true);
            return new MessageResponseDto { Success = true, Message = $"Operator '{op.CompanyName}' approved." };
        }

        public async Task<MessageResponseDto> DisableOperatorAsync(int adminId, int operatorId)
        {
            var op = await _db.BusOperators.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == operatorId)
                ?? throw new InvalidOperationException("Operator not found.");

            op.Status = ApprovalStatus.Disabled;
            op.User.IsActive = false;

            // Also set all their buses to Down
            var buses = await _db.Buses.Where(b => b.OperatorId == operatorId).ToListAsync();
            buses.ForEach(b => b.Status = BusStatus.Down);

            await _db.SaveChangesAsync();
            _ = _email.SendOperatorApprovalAsync(op.User.Email, op.CompanyName, false);
            return new MessageResponseDto { Success = true, Message = $"Operator '{op.CompanyName}' disabled." };
        }

        public async Task<List<BusPendingDto>> GetBusesAsync(string? status)
        {
            var query = _db.Buses
                .Include(b => b.Operator).ThenInclude(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BusStatus>(status, true, out var busStatus))
                query = query.Where(b => b.Status == busStatus);

            return await query.Select(b => new BusPendingDto
            {
                Id = b.Id,
                BusName = b.BusName,
                RegistrationNumber = b.RegistrationNumber,
                BusType = b.BusType,
                OperatorName = b.Operator.User.Name,
                CompanyName = b.Operator.CompanyName,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt
            }).ToListAsync();
        }

        public async Task<MessageResponseDto> ApproveBusAsync(int adminId, int busId)
        {
            var bus = await _db.Buses.FindAsync(busId)
                ?? throw new InvalidOperationException("Bus not found.");

            bus.Status = BusStatus.Active;
            bus.ApprovedByAdminId = adminId;
            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = $"Bus '{bus.BusName}' approved and set to Active." };
        }

        public async Task<MessageResponseDto> DisableBusAsync(int adminId, int busId)
        {
            var bus = await _db.Buses.FindAsync(busId)
                ?? throw new InvalidOperationException("Bus not found.");

            bus.Status = BusStatus.Removed;
            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = $"Bus '{bus.BusName}' removed." };
        }

        public async Task<List<RouteDto>> GetRoutesAsync()
        {
            return await _db.Routes
                .Include(r => r.Schedules)
                .Select(r => new RouteDto
                {
                    Id = r.Id,
                    SourceCity = r.SourceCity,
                    DestinationCity = r.DestinationCity,
                    IsActive = r.IsActive,
                    TotalSchedules = r.Schedules.Count
                }).ToListAsync();
        }

        public async Task<RouteDto> CreateRouteAsync(int adminId, CreateRouteDto dto)
        {
            var exists = await _db.Routes.AnyAsync(r =>
                r.SourceCity.ToLower() == dto.SourceCity.ToLower() &&
                r.DestinationCity.ToLower() == dto.DestinationCity.ToLower());

            if (exists)
                throw new InvalidOperationException("This route already exists.");

            var route = new BusRoute
            {
                SourceCity = dto.SourceCity,
                DestinationCity = dto.DestinationCity,
                CreatedByAdminId = adminId,
                IsActive = true
            };

            _db.Routes.Add(route);
            await _db.SaveChangesAsync();

            return new RouteDto
            {
                Id = route.Id,
                SourceCity = route.SourceCity,
                DestinationCity = route.DestinationCity,
                IsActive = route.IsActive,
                TotalSchedules = 0
            };
        }

        public async Task<MessageResponseDto> ToggleRouteAsync(int routeId)
        {
            var route = await _db.Routes.FindAsync(routeId)
                ?? throw new InvalidOperationException("Route not found.");

            route.IsActive = !route.IsActive;
            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = $"Route is now {(route.IsActive ? "active" : "inactive")}." };
        }

        public async Task<RevenueDto> GetRevenueAsync()
        {
            var bookings = await _db.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Bus).ThenInclude(bus => bus.Operator).ThenInclude(o => o.User)
                .Where(b => b.Status == BookingStatus.Confirmed)
                .ToListAsync();

            var cancellations = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled);

            var byOperator = bookings
                .GroupBy(b => b.Schedule.Bus.OperatorId)
                .Select(g => new OperatorRevenueDto
                {
                    OperatorId = g.Key,
                    OperatorName = g.First().Schedule.Bus.Operator.User.Name,
                    CompanyName = g.First().Schedule.Bus.Operator.CompanyName,
                    TotalBookings = g.Count(),
                    GrossRevenue = g.Sum(b => b.TotalAmount + b.PlatformFee),
                    PlatformFeeCollected = g.Sum(b => b.PlatformFee)
                }).ToList();

            return new RevenueDto
            {
                TotalRevenue = bookings.Sum(b => b.TotalAmount + b.PlatformFee),
                TotalPlatformFee = bookings.Sum(b => b.PlatformFee),
                TotalBookings = bookings.Count,
                TotalCancellations = cancellations,
                ByOperator = byOperator
            };
        }

        public async Task<List<PlatformSettingDto>> GetSettingsAsync()
        {
            return await _db.PlatformSettings.Select(s => new PlatformSettingDto
            {
                Key = s.Key,
                Value = s.Value,
                Description = s.Description
            }).ToListAsync();
        }

        public async Task<MessageResponseDto> UpdateSettingAsync(int adminId, string key, string value)
        {
            var setting = await _db.PlatformSettings.FirstOrDefaultAsync(s => s.Key == key)
                ?? throw new InvalidOperationException($"Setting '{key}' not found.");

            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedByAdminId = adminId;
            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = $"Setting '{key}' updated to '{value}'." };
        }
    }
}
