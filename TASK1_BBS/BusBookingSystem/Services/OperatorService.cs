using BusBookingSystem.Data;
using BusBookingSystem.DTOs;
using BusBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Services
{
    public interface IOperatorService
    {
        Task<List<BusDto>> GetMyBusesAsync(int userId);
        Task<BusDto> AddBusAsync(int userId, AddBusDto dto);
        Task<MessageResponseDto> BringDownBusAsync(int userId, int busId);
        Task<List<LayoutDto>> GetLayoutsAsync(int userId);
        Task<LayoutDto> UploadLayoutAsync(int userId, UploadLayoutDto dto);
        Task<List<ScheduleDto>> GetSchedulesAsync(int userId);
        Task<ScheduleDto> CreateScheduleAsync(int userId, CreateScheduleDto dto);
        Task<MessageResponseDto> UpdatePriceAsync(int userId, int scheduleId, UpdatePriceDto dto);
        Task<List<BookingListDto>> GetOperatorBookingsAsync(int userId);
        Task<OperatorListDto> GetMyProfileAsync(int userId);
    }

    public class OperatorService : IOperatorService
    {
        private readonly AppDbContext _db;
        public OperatorService(AppDbContext db) => _db = db;

        private async Task<BusOperator> GetApprovedOperatorAsync(int userId)
        {
            var op = await _db.BusOperators
                .Include(o => o.User)
                .Include(o => o.Offices)
                .FirstOrDefaultAsync(o => o.UserId == userId)
                ?? throw new UnauthorizedAccessException("Operator profile not found.");

            if (op.Status != ApprovalStatus.Approved)
                throw new UnauthorizedAccessException("Your account is not yet approved by Admin.");

            return op;
        }

        public async Task<List<BusDto>> GetMyBusesAsync(int userId)
        {
            var op = await GetApprovedOperatorAsync(userId);
            return await _db.Buses
                .Include(b => b.Layout)
                .Where(b => b.OperatorId == op.Id)
                .Select(b => new BusDto
                {
                    Id = b.Id,
                    BusName = b.BusName,
                    RegistrationNumber = b.RegistrationNumber,
                    BusType = b.BusType,
                    Status = b.Status.ToString(),
                    LayoutName = b.Layout != null ? b.Layout.Name : null,
                    LayoutId = b.LayoutId,
                    CreatedAt = b.CreatedAt
                }).ToListAsync();
        }

        public async Task<BusDto> AddBusAsync(int userId, AddBusDto dto)
        {
            var op = await GetApprovedOperatorAsync(userId);

            if (await _db.Buses.AnyAsync(b => b.RegistrationNumber == dto.RegistrationNumber))
                throw new InvalidOperationException("A bus with this registration number already exists.");

            var bus = new Bus
            {
                OperatorId = op.Id,
                BusName = dto.BusName,
                RegistrationNumber = dto.RegistrationNumber,
                BusType = dto.BusType,
                LayoutId = dto.LayoutId,
                Status = BusStatus.Pending
            };

            _db.Buses.Add(bus);
            await _db.SaveChangesAsync();

            return new BusDto
            {
                Id = bus.Id,
                BusName = bus.BusName,
                RegistrationNumber = bus.RegistrationNumber,
                BusType = bus.BusType,
                Status = bus.Status.ToString(),
                LayoutId = bus.LayoutId,
                CreatedAt = bus.CreatedAt
            };
        }

        public async Task<MessageResponseDto> BringDownBusAsync(int userId, int busId)
        {
            var op = await GetApprovedOperatorAsync(userId);
            var bus = await _db.Buses.FirstOrDefaultAsync(b => b.Id == busId && b.OperatorId == op.Id)
                ?? throw new InvalidOperationException("Bus not found.");

            if (bus.Status == BusStatus.Removed)
                throw new InvalidOperationException("Bus is already removed.");

            bus.Status = BusStatus.Down;
            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = "Bus brought down. Awaiting admin confirmation." };
        }

        public async Task<List<LayoutDto>> GetLayoutsAsync(int userId)
        {
            var op = await GetApprovedOperatorAsync(userId);

            return await _db.BusLayouts
                .Where(l => l.IsGlobal || l.CreatedByOperatorId == op.Id)
                .Select(l => new LayoutDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    TotalRows = l.TotalRows,
                    SeatsPerRow = l.SeatsPerRow,
                    HasUpperDeck = l.HasUpperDeck,
                    IsGlobal = l.IsGlobal,
                    LayoutJson = l.LayoutJson
                }).ToListAsync();
        }

        public async Task<LayoutDto> UploadLayoutAsync(int userId, UploadLayoutDto dto)
        {
            var op = await GetApprovedOperatorAsync(userId);

            var layout = new BusLayout
            {
                Name = dto.Name,
                Description = dto.Description,
                TotalRows = dto.TotalRows,
                SeatsPerRow = dto.SeatsPerRow,
                HasUpperDeck = dto.HasUpperDeck,
                LayoutJson = dto.LayoutJson,
                CreatedByOperatorId = op.Id,
                IsGlobal = false
            };

            _db.BusLayouts.Add(layout);
            await _db.SaveChangesAsync();

            return new LayoutDto
            {
                Id = layout.Id,
                Name = layout.Name,
                Description = layout.Description,
                TotalRows = layout.TotalRows,
                SeatsPerRow = layout.SeatsPerRow,
                HasUpperDeck = layout.HasUpperDeck,
                IsGlobal = layout.IsGlobal,
                LayoutJson = layout.LayoutJson
            };
        }

        public async Task<List<ScheduleDto>> GetSchedulesAsync(int userId)
        {
            var op = await GetApprovedOperatorAsync(userId);
            return await _db.BusSchedules
                .Include(s => s.Bus)
                .Include(s => s.Route)
                .Include(s => s.Bookings)
                .Where(s => s.Bus.OperatorId == op.Id)
                .OrderByDescending(s => s.DepartureTime)
                .Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    BusId = s.BusId,
                    BusName = s.Bus.BusName,
                    RouteName = s.Route.SourceCity + " → " + s.Route.DestinationCity,
                    Source = s.Route.SourceCity,
                    Destination = s.Route.DestinationCity,
                    DepartureTime = s.DepartureTime,
                    ArrivalTime = s.ArrivalTime,
                    PricePerSeat = s.PricePerSeat,
                    IsCancelled = s.IsCancelled,
                    TotalBookings = s.Bookings.Count(b => b.Status == BookingStatus.Confirmed)
                }).ToListAsync();
        }

        public async Task<ScheduleDto> CreateScheduleAsync(int userId, CreateScheduleDto dto)
        {
            var op = await GetApprovedOperatorAsync(userId);

            var bus = await _db.Buses.FirstOrDefaultAsync(b => b.Id == dto.BusId && b.OperatorId == op.Id && b.Status == BusStatus.Active)
                ?? throw new InvalidOperationException("Bus not found or not active.");

            var route = await _db.Routes.FirstOrDefaultAsync(r => r.Id == dto.RouteId && r.IsActive)
                ?? throw new InvalidOperationException("Route not found or inactive.");

            var schedule = new BusSchedule
            {
                BusId = dto.BusId,
                RouteId = dto.RouteId,
                DepartureTime = DateTime.SpecifyKind(dto.DepartureTime, DateTimeKind.Utc),
                ArrivalTime   = DateTime.SpecifyKind(dto.ArrivalTime,   DateTimeKind.Utc),
                PricePerSeat  = dto.PricePerSeat
            };

            _db.BusSchedules.Add(schedule);
            await _db.SaveChangesAsync();

            return new ScheduleDto
            {
                Id = schedule.Id,
                BusId = schedule.BusId,
                BusName = bus.BusName,
                RouteName = route.SourceCity + " → " + route.DestinationCity,
                Source = route.SourceCity,
                Destination = route.DestinationCity,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                PricePerSeat = schedule.PricePerSeat,
                IsCancelled = false,
                TotalBookings = 0
            };
        }

        public async Task<MessageResponseDto> UpdatePriceAsync(int userId, int scheduleId, UpdatePriceDto dto)
        {
            var op = await GetApprovedOperatorAsync(userId);
            var schedule = await _db.BusSchedules
                .Include(s => s.Bus)
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.Bus.OperatorId == op.Id)
                ?? throw new InvalidOperationException("Schedule not found.");

            schedule.PricePerSeat = dto.PricePerSeat;
            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = "Price updated successfully." };
        }

        public async Task<List<BookingListDto>> GetOperatorBookingsAsync(int userId)
        {
            var op = await GetApprovedOperatorAsync(userId);

            return await _db.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Route)
                .Include(b => b.Schedule).ThenInclude(s => s.Bus).ThenInclude(bus => bus.Operator).ThenInclude(o => o.User)
                .Include(b => b.Customer)
                .Include(b => b.Passengers)
                .Where(b => b.Schedule.Bus.OperatorId == op.Id)
                .OrderByDescending(b => b.BookedAt)
                .Select(b => new BookingListDto
                {
                    BookingId = b.Id,
                    BusName = b.Schedule.Bus.BusName,
                    OperatorName = b.Schedule.Bus.Operator.CompanyName,
                    CustomerName = b.Customer.Name,
                    Source = b.Schedule.Route.SourceCity,
                    Destination = b.Schedule.Route.DestinationCity,
                    DepartureTime = b.Schedule.DepartureTime,
                    BookedAt = b.BookedAt,
                    TotalAmount = b.TotalAmount + b.PlatformFee,
                    Status = b.Status.ToString(),
                    RefundAmount = b.RefundAmount,
                    PassengerCount = b.Passengers.Count,
                    SeatNumbers = b.Passengers.Select(p => p.SeatNumber).ToList()
                }).ToListAsync();
        }

        public async Task<OperatorListDto> GetMyProfileAsync(int userId)
        {
            var op = await _db.BusOperators
                .Include(o => o.User)
                .Include(o => o.Offices)
                .Include(o => o.Buses)
                .FirstOrDefaultAsync(o => o.UserId == userId)
                ?? throw new InvalidOperationException("Operator not found.");

            return new OperatorListDto
            {
                Id = op.Id,
                UserId = op.UserId,
                OperatorName = op.User.Name,
                CompanyName = op.CompanyName,
                Email = op.User.Email,
                Phone = op.User.Phone,
                GSTNumber = op.GSTNumber,
                Status = op.Status.ToString(),
                RegisteredAt = op.RegisteredAt,
                Offices = op.Offices.Select(o => new OfficeDto
                {
                    District = o.District,
                    City = o.City,
                    Address = o.Address
                }).ToList(),
                TotalBuses = op.Buses.Count
            };
        }
    }
}
