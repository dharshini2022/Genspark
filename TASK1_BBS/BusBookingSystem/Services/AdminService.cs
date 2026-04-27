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
        Task<MessageResponseDto> CancelScheduleAsync(int adminId, int scheduleId);
        Task<CancelResponseDto> CancelBookingAsync(int adminId, int bookingId, CancelBookingDto dto);
    }

    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private static readonly string[] DefaultPhotos = { "10fb39a7-8c38-46ba-bdb8-181724599a06.jpg", "18168f51-1d29-4440-a1e5-e6cc91e574c9.webp" };

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
            try { await _email.SendOperatorApprovalAsync(op.User.Email, op.CompanyName, true); } catch { }
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
            try { await _email.SendOperatorApprovalAsync(op.User.Email, op.CompanyName, false); } catch { }
            return new MessageResponseDto { Success = true, Message = $"Operator '{op.CompanyName}' disabled." };
        }

        public async Task<List<BusPendingDto>> GetBusesAsync(string? status)
        {
            var query = _db.Buses
                .Include(b => b.Operator).ThenInclude(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BusStatus>(status, true, out var busStatus))
                query = query.Where(b => b.Status == busStatus);

            var busEntities = await query.ToListAsync();
            return busEntities.Select(b => new BusPendingDto
            {
                Id = b.Id,
                BusName = b.BusName,
                RegistrationNumber = b.RegistrationNumber,
                BusType = b.BusType,
                OperatorName = b.Operator.User.Name,
                CompanyName = b.Operator.CompanyName,
                Status = b.Status.ToString(),
                Features = b.Features != null ? b.Features.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new(),
                Photos = (string.IsNullOrWhiteSpace(b.Photos) ? DefaultPhotos : b.Photos.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(p => p.StartsWith("/") ? p : "/img/" + p).ToList(),
                CreatedAt = b.CreatedAt
            }).ToList();
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
            var routes = await _db.Routes.Include(r => r.Schedules).ToListAsync();
            
            // Auto-fix: Ensure bi-directional consistency
            bool changed = false;
            var currentRoutes = routes.ToList();
            foreach (var r in currentRoutes)
            {
                var hasReverse = currentRoutes.Any(rev => 
                    rev.SourceCity.ToLower() == r.DestinationCity.ToLower() && 
                    rev.DestinationCity.ToLower() == r.SourceCity.ToLower());
                
                if (!hasReverse)
                {
                    var reverse = new BusRoute
                    {
                        SourceCity = r.DestinationCity,
                        DestinationCity = r.SourceCity,
                        CreatedByAdminId = r.CreatedByAdminId,
                        IsActive = r.IsActive
                    };
                    _db.Routes.Add(reverse);
                    changed = true;
                }
            }
            if (changed) 
            {
                await _db.SaveChangesAsync();
                routes = await _db.Routes.Include(r => r.Schedules).ToListAsync(); // reload
            }

            return routes.Select(r => new RouteDto
            {
                Id = r.Id,
                SourceCity = r.SourceCity,
                DestinationCity = r.DestinationCity,
                IsActive = r.IsActive,
                TotalSchedules = r.Schedules.Count
            }).ToList();
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

            // Create reverse route if it doesn't exist
            var reverseExists = await _db.Routes.AnyAsync(r =>
                r.SourceCity.ToLower() == dto.DestinationCity.ToLower() &&
                r.DestinationCity.ToLower() == dto.SourceCity.ToLower());

            if (!reverseExists)
            {
                var reverseRoute = new BusRoute
                {
                    SourceCity = dto.DestinationCity,
                    DestinationCity = dto.SourceCity,
                    CreatedByAdminId = adminId,
                    IsActive = true
                };
                _db.Routes.Add(reverseRoute);
            }

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
                    GrossRevenue = g.Sum(b => b.TotalAmount),
                    PlatformFeeCollected = g.Sum(b => b.PlatformFee)
                }).ToList();

            return new RevenueDto
            {
                TotalRevenue = bookings.Sum(b => b.PlatformFee),
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

        public async Task<MessageResponseDto> CancelScheduleAsync(int adminId, int scheduleId)
        {
            var schedule = await _db.BusSchedules
                .Include(s => s.Bus)
                .Include(s => s.Route)
                .Include(s => s.Bookings).ThenInclude(b => b.Customer)
                .Include(s => s.Bookings).ThenInclude(b => b.Payment)
                .FirstOrDefaultAsync(s => s.Id == scheduleId)
                ?? throw new InvalidOperationException("Schedule not found.");

            if (schedule.IsCancelled)
                throw new InvalidOperationException("Schedule is already cancelled.");

            schedule.IsCancelled = true;

            foreach (var booking in schedule.Bookings.Where(b => b.Status == BookingStatus.Confirmed))
            {
                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = "Service cancelled by system administrator.";
                booking.RefundAmount = booking.TotalAmount + booking.PlatformFee + booking.Gst;

                if (booking.Payment != null)
                    booking.Payment.Status = PaymentStatus.Refunded;

                if (booking.Customer != null)
                {
                    try
                    {
                        await _email.SendBusServiceCancellationAsync(
                            booking.Customer.Email,
                            booking.Customer.Name,
                            schedule.Bus.BusName,
                            schedule.Route.SourceCity,
                            schedule.Route.DestinationCity,
                            schedule.DepartureTime);
                    }
                    catch { }
                }
            }

            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = "Schedule cancelled by admin." };
        }

        public async Task<CancelResponseDto> CancelBookingAsync(int adminId, int bookingId, CancelBookingDto dto)
        {
            var booking = await _db.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Bus)
                .Include(b => b.Schedule).ThenInclude(s => s.Route)
                .Include(b => b.Payment)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new InvalidOperationException("Booking not found.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking already cancelled.");

            decimal totalPaid = booking.TotalAmount + booking.PlatformFee + booking.Gst;
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.RefundAmount = totalPaid;
            booking.CancellationReason = dto.Reason;

            if (booking.Payment != null)
                booking.Payment.Status = PaymentStatus.Refunded;

            await _db.SaveChangesAsync();

            if (booking.Customer != null)
            {
                try
                {
                    await _email.SendBookingCancellationAsync(booking.Customer.Email, booking.Customer.Name, new CancellationEmailDto
                    {
                        BookingId = booking.Id,
                        BusName = booking.Schedule.Bus?.BusName ?? "Bus",
                        Source = booking.Schedule.Route?.SourceCity ?? string.Empty,
                        Destination = booking.Schedule.Route?.DestinationCity ?? string.Empty,
                        RefundAmount = totalPaid,
                        RefundPolicy = "Admin Cancellation: 100% refund"
                    });
                }
                catch { }
            }

            return new CancelResponseDto { Success = true, Message = "Booking cancelled by admin.", RefundAmount = totalPaid };
        }
    }
}
