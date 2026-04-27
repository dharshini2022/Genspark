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

    public class OperatorService : IOperatorService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private static readonly string[] DefaultPhotos = { "10fb39a7-8c38-46ba-bdb8-181724599a06.jpg", "18168f51-1d29-4440-a1e5-e6cc91e574c9.webp" };

        public OperatorService(AppDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

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
            var busEntities = await _db.Buses
                .Include(b => b.Layout)
                .Where(b => b.OperatorId == op.Id)
                .ToListAsync();

            return busEntities.Select(b => new BusDto
            {
                Id = b.Id,
                BusName = b.BusName,
                RegistrationNumber = b.RegistrationNumber,
                BusType = b.BusType,
                Status = b.Status.ToString(),
                LayoutName = b.Layout != null ? b.Layout.Name : null,
                LayoutId = b.LayoutId,
                Features = b.Features != null ? b.Features.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new(),
                Photos = (string.IsNullOrWhiteSpace(b.Photos) ? DefaultPhotos : b.Photos.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(p => p.StartsWith("/") ? p : "/img/" + p).ToList(),
                CreatedAt = b.CreatedAt
            }).ToList();
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
                Features = dto.Features != null ? string.Join(',', dto.Features) : null,
                Photos = dto.Photos != null ? string.Join(',', dto.Photos) : null,
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
                Features = dto.Features,
                Photos = (dto.Photos != null && dto.Photos.Count > 0 ? dto.Photos : DefaultPhotos.ToList()).Select(p => p.StartsWith("/") ? p : "/img/" + p).ToList(),
                CreatedAt = bus.CreatedAt
            };
        }

        public async Task<MessageResponseDto> BringUpBusAsync(int userId, int busId)
        {
            var op = await GetApprovedOperatorAsync(userId);
            var bus = await _db.Buses.FirstOrDefaultAsync(b => b.Id == busId && b.OperatorId == op.Id)
                ?? throw new InvalidOperationException("Bus not found.");

            if (bus.Status != BusStatus.Down)
                throw new InvalidOperationException("Only buses that are 'Down' can be brought up.");

            bus.Status = BusStatus.Pending; // Requires admin approval
            await _db.SaveChangesAsync();

            return new MessageResponseDto { Success = true, Message = "Bus status changed to Pending. Awaiting admin approval." };
        }

        public async Task<MessageResponseDto> BringDownBusAsync(int userId, int busId)
        {
            var op = await GetApprovedOperatorAsync(userId);
            var bus = await _db.Buses.FirstOrDefaultAsync(b => b.Id == busId && b.OperatorId == op.Id)
                ?? throw new InvalidOperationException("Bus not found.");

            if (bus.Status == BusStatus.Removed)
                throw new InvalidOperationException("Bus is already removed.");

            bus.Status = BusStatus.Down;

            var activeSchedules = await _db.BusSchedules
                .Include(s => s.Route)
                .Include(s => s.Bookings).ThenInclude(b => b.Customer)
                .Include(s => s.Bookings).ThenInclude(b => b.Payment)
                .Where(s => s.BusId == busId && !s.IsCancelled && s.DepartureTime > DateTime.UtcNow)
                .ToListAsync();

            foreach (var schedule in activeSchedules)
            {
                schedule.IsCancelled = true;
                foreach (var booking in schedule.Bookings.Where(b => b.Status == BookingStatus.Confirmed))
                {
                    booking.Status = BookingStatus.Cancelled;
                    booking.CancelledAt = DateTime.UtcNow;
                    booking.CancellationReason = "Bus is taken down by operator.";
                    
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
                                bus.BusName,
                                schedule.Route.SourceCity,
                                schedule.Route.DestinationCity,
                                schedule.DepartureTime);
                        }
                        catch { /* Ignore email failure */ }
                    }
                }
            }

            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = "Bus brought down. Active schedules cancelled, passengers refunded and notified." };
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
                    TotalAmount = b.TotalAmount,
                    PlatformFee = b.PlatformFee,
                    Gst = b.Gst,
                    Status = b.Status.ToString(),
                    RefundAmount = b.RefundAmount,
                    PassengerCount = b.Passengers.Count,
                    SeatNumbers = b.Passengers.Select(p => p.SeatNumber).ToList()
                }).ToListAsync();
        }

        public async Task<MessageResponseDto> CancelScheduleAsync(int userId, int scheduleId)
        {
            var op = await GetApprovedOperatorAsync(userId);
            var schedule = await _db.BusSchedules
                .Include(s => s.Bus)
                .Include(s => s.Route)
                .Include(s => s.Bookings).ThenInclude(b => b.Customer)
                .Include(s => s.Bookings).ThenInclude(b => b.Payment)
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.Bus.OperatorId == op.Id)
                ?? throw new InvalidOperationException("Schedule not found.");

            if (schedule.IsCancelled)
                throw new InvalidOperationException("Schedule is already cancelled.");

            schedule.IsCancelled = true;

            // Cancel all bookings and notify users
            foreach (var booking in schedule.Bookings.Where(b => b.Status == BookingStatus.Confirmed))
            {
                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = "Service cancelled by operator.";
                
                // Full refund for service cancellation
                decimal totalPaid = booking.TotalAmount + booking.PlatformFee + booking.Gst;
                booking.RefundAmount = totalPaid;

                if (booking.Payment != null)
                    booking.Payment.Status = PaymentStatus.Refunded;

                // Send email to the customer
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
                    catch { /* Log error but continue with other users */ }
                }
            }

            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = "Schedule cancelled and passengers notified." };
        }

        public async Task<CancelResponseDto> CancelBookingAsync(int userId, int bookingId, CancelBookingDto dto)
        {
            var op = await GetApprovedOperatorAsync(userId);
            var booking = await _db.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Bus)
                .Include(b => b.Schedule).ThenInclude(s => s.Route)
                .Include(b => b.Payment)
                .Include(b => b.Customer)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.Schedule.Bus.OperatorId == op.Id)
                ?? throw new InvalidOperationException("Booking not found on your buses.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking is already cancelled.");

            // Calculate refund based on policy (simplified for operator cancellation)
            // If operator cancels a single booking, we assume full refund is appropriate or use standard policy.
            // Let's use the standard policy from DB if possible, or just full refund if it's the operator's choice.
            // Requirement says "operator admin" - if it's the operator's fault, full refund is usually better.
            
            decimal totalPaid = booking.TotalAmount + booking.PlatformFee;
            decimal refundAmount = totalPaid; // Default to full refund when operator cancels

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.RefundAmount = refundAmount;
            booking.CancellationReason = dto.Reason;

            if (booking.Payment != null)
                booking.Payment.Status = PaymentStatus.Refunded;

            await _db.SaveChangesAsync();

            var cancelResponse = new CancelResponseDto
            {
                Success = true,
                Message = "Booking cancelled successfully.",
                RefundAmount = refundAmount,
                RefundPolicy = "Operator Cancellation: 100% refund"
            };

            // Notify user
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
                        RefundAmount = refundAmount,
                        RefundPolicy = cancelResponse.RefundPolicy
                    });
                }
                catch { }
            }

            return cancelResponse;
        }
        public async Task<OperatorDetailedRevenueDto> GetDetailedRevenueAsync(int userId, DateTime? startDate, DateTime? endDate, int? busId)
        {
            var op = await GetApprovedOperatorAsync(userId);
            
            var query = _db.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Bus)
                .Include(b => b.Schedule).ThenInclude(s => s.Route)
                .Where(b => b.Schedule.Bus.OperatorId == op.Id);

            if (startDate.HasValue)
                query = query.Where(b => b.Schedule.DepartureTime >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(b => b.Schedule.DepartureTime <= endDate.Value);
                
            if (busId.HasValue)
                query = query.Where(b => b.Schedule.BusId == busId.Value);

            var bookings = await query.ToListAsync();

            var result = new OperatorDetailedRevenueDto
            {
                TotalRevenue = bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.TotalAmount),
                TotalBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                TotalCancellations = bookings.Count(b => b.Status == BookingStatus.Cancelled)
            };

            var byBus = bookings.GroupBy(b => new { b.Schedule.BusId, b.Schedule.Bus.BusName, b.Schedule.Bus.RegistrationNumber })
                .Select(g => new BusRevenueDto
                {
                    BusId = g.Key.BusId,
                    BusName = g.Key.BusName,
                    RegistrationNumber = g.Key.RegistrationNumber,
                    TotalBookings = g.Count(b => b.Status == BookingStatus.Confirmed),
                    Revenue = g.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.TotalAmount)
                }).ToList();
            
            result.ByBus = byBus;

            var schedulesQuery = _db.BusSchedules
                .Include(s => s.Bus).ThenInclude(b => b.Layout)
                .Include(s => s.Route)
                .Include(s => s.Bookings).ThenInclude(b => b.Passengers)
                .Where(s => s.Bus.OperatorId == op.Id);

            if (startDate.HasValue) schedulesQuery = schedulesQuery.Where(s => s.DepartureTime >= startDate.Value);
            if (endDate.HasValue) schedulesQuery = schedulesQuery.Where(s => s.DepartureTime <= endDate.Value);
            if (busId.HasValue) schedulesQuery = schedulesQuery.Where(s => s.BusId == busId.Value);

            var schedulesList = await schedulesQuery.ToListAsync();

            var bySchedule = schedulesList.Select(s => 
            {
                int totalSeats = s.Bus.Layout != null ? s.Bus.Layout.TotalRows * s.Bus.Layout.SeatsPerRow : 40;
                int bookedSeats = s.Bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.Passengers?.Count ?? 0);
                
                return new ScheduleRevenueDto
                {
                    ScheduleId = s.Id,
                    BusId = s.BusId,
                    BusName = s.Bus.BusName,
                    RouteName = s.Route.SourceCity + " → " + s.Route.DestinationCity,
                    DepartureTime = s.DepartureTime,
                    TotalSeats = totalSeats,
                    BookedSeats = bookedSeats,
                    OccupancyPercentage = totalSeats > 0 ? Math.Round((double)bookedSeats / totalSeats * 100, 2) : 0,
                    Revenue = s.Bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.TotalAmount)
                };
            }).ToList();

            result.BySchedule = bySchedule;
            
            return result;
        }

        public async Task<List<SchedulePassengerDto>> GetSchedulePassengersAsync(int userId, int scheduleId)
        {
            var op = await GetApprovedOperatorAsync(userId);
            var schedule = await _db.BusSchedules
                .Include(s => s.Bus).ThenInclude(b => b.Operator).ThenInclude(o => o.Offices)
                .Include(s => s.Route)
                .Include(s => s.Bookings).ThenInclude(b => b.Passengers)
                .Include(s => s.Bookings).ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.Bus.OperatorId == op.Id)
                ?? throw new InvalidOperationException("Schedule not found or unauthorized.");

            var sourceCity = schedule.Route?.SourceCity ?? "";
            var office = schedule.Bus.Operator.Offices.FirstOrDefault(o => o.City.Trim().Equals(sourceCity.Trim(), StringComparison.OrdinalIgnoreCase));
            var boardingAddress = office?.Address ?? $"{sourceCity} Bus Stand";

            var result = new List<SchedulePassengerDto>();
            foreach(var booking in schedule.Bookings.Where(b => b.Status == BookingStatus.Confirmed))
            {
                string contact = booking.Customer.Phone ?? "";
                if (contact.Length > 4)
                    contact = new string('*', contact.Length - 4) + contact.Substring(contact.Length - 4);

                foreach(var p in booking.Passengers)
                {
                    result.Add(new SchedulePassengerDto
                    {
                        BookingId = booking.Id,
                        PassengerName = p.PassengerName,
                        SeatNumber = p.SeatNumber,
                        BoardingPoint = boardingAddress,
                        MaskedContact = contact
                    });
                }
            }
            return result;
        }

        public async Task<List<OfficeDto>> GetOfficesAsync(int userId)
        {
            var op = await GetApprovedOperatorAsync(userId);
            return await _db.OperatorOffices
                .Where(o => o.OperatorId == op.Id)
                .Select(o => new OfficeDto
                {
                    Id = o.Id,
                    District = o.District,
                    City = o.City,
                    Address = o.Address
                }).ToListAsync();
        }

        public async Task<MessageResponseDto> AddOfficeAsync(int userId, OfficeDto dto)
        {
            var op = await GetApprovedOperatorAsync(userId);
            _db.OperatorOffices.Add(new OperatorOffice
            {
                OperatorId = op.Id,
                District = dto.District ?? "",
                City = dto.City,
                Address = dto.Address
            });
            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = "Office added successfully." };
        }

        public async Task<MessageResponseDto> UpdateOfficeAsync(int userId, int officeId, OfficeDto dto)
        {
            var op = await GetApprovedOperatorAsync(userId);
            var office = await _db.OperatorOffices.FirstOrDefaultAsync(o => o.Id == officeId && o.OperatorId == op.Id);
            if (office == null) throw new InvalidOperationException("Office not found.");
            
            office.District = dto.District ?? office.District;
            office.City = dto.City;
            office.Address = dto.Address;
            await _db.SaveChangesAsync();
            return new MessageResponseDto { Success = true, Message = "Office updated successfully." };
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
