using BusBookingSystem.Data;
using BusBookingSystem.DTOs;
using BusBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Services
{
    public interface ICustomerService
    {
        Task<List<BusSearchResultDto>> SearchBusesAsync(string source, string destination, DateOnly date);
        Task<SeatBlockResponseDto> BlockSeatsAsync(int userId, SeatBlockRequestDto dto);
        Task<BookingResponseDto> CreateBookingAsync(int userId, CreateBookingDto dto);
        Task<PaymentResponseDto> ProcessPaymentAsync(int userId, PaymentRequestDto dto);
        Task<List<BookingListDto>> GetMyBookingsAsync(int userId);
        Task<BookingResponseDto> GetBookingDetailAsync(int userId, int bookingId);
        Task<CancelResponseDto> CancelBookingAsync(int userId, int bookingId, CancelBookingDto dto);
        Task<ProfileDto> GetProfileAsync(int userId);
        Task<ProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    }

    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;

        public CustomerService(AppDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        private async Task<decimal> GetPlatformFeePercentAsync()
        {
            var setting = await _db.PlatformSettings.FirstOrDefaultAsync(s => s.Key == "PlatformFeePercent");
            return setting != null ? decimal.Parse(setting.Value) : 5m;
        }

        public async Task<List<BusSearchResultDto>> SearchBusesAsync(string source, string destination, DateOnly date)
        {
            var feePercent = await GetPlatformFeePercentAsync();
            var now = DateTime.UtcNow;

            // Convert DateOnly → explicit UTC DateTime range so Npgsql 6+ can compare
            // against 'timestamp with time zone' columns without ambiguity.
            var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDay   = startOfDay.AddDays(1);

            var schedules = await _db.BusSchedules
                .Include(s => s.Bus).ThenInclude(b => b.Operator).ThenInclude(o => o.User)
                .Include(s => s.Bus).ThenInclude(b => b.Layout)
                .Include(s => s.Route)
                .Include(s => s.Bookings).ThenInclude(b => b.Passengers)
                .Include(s => s.SeatBlocks)
                .Where(s =>
                    s.Route.SourceCity.ToLower() == source.ToLower() &&
                    s.Route.DestinationCity.ToLower() == destination.ToLower() &&
                    s.DepartureTime >= startOfDay && s.DepartureTime < endOfDay &&
                    !s.IsCancelled &&
                    s.Bus.Status == BusStatus.Active)
                .ToListAsync();


            var result = new List<BusSearchResultDto>();

            foreach (var s in schedules)
            {
                var bookedSeats = s.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed)
                    .SelectMany(b => b.Passengers.Select(p => p.SeatNumber))
                    .ToList();

                var blockedSeats = s.SeatBlocks
                    .Where(sb => !sb.IsReleased && sb.ExpiresAt > now)
                    .Select(sb => sb.SeatNumber)
                    .ToList();

                int totalSeats = s.Bus.Layout?.TotalRows * s.Bus.Layout?.SeatsPerRow ?? 40;
                int available = totalSeats - bookedSeats.Count - blockedSeats.Count;

                decimal platformFee = Math.Round(s.PricePerSeat * feePercent / 100, 2);

                result.Add(new BusSearchResultDto
                {
                    ScheduleId = s.Id,
                    BusId = s.BusId,
                    BusName = s.Bus.BusName,
                    BusType = s.Bus.BusType,
                    OperatorName = s.Bus.Operator.User.Name,
                    Source = s.Route.SourceCity,
                    Destination = s.Route.DestinationCity,
                    DepartureTime = s.DepartureTime,
                    ArrivalTime = s.ArrivalTime,
                    PricePerSeat = s.PricePerSeat,
                    PlatformFee = platformFee,
                    TotalPrice = s.PricePerSeat + platformFee,
                    AvailableSeats = Math.Max(0, available),
                    TotalSeats = totalSeats,
                    LayoutJson = s.Bus.Layout?.LayoutJson ?? string.Empty,
                    BookedSeats = bookedSeats,
                    BlockedSeats = blockedSeats
                });
            }

            return result;
        }

        public async Task<SeatBlockResponseDto> BlockSeatsAsync(int userId, SeatBlockRequestDto dto)
        {
            var now = DateTime.UtcNow;

            // Remove expired blocks
            var expired = await _db.SeatBlocks
                .Where(sb => sb.ScheduleId == dto.ScheduleId && !sb.IsReleased && sb.ExpiresAt <= now)
                .ToListAsync();
            expired.ForEach(e => e.IsReleased = true);

            // Check if any requested seat is already booked or blocked
            var bookedSeats = await _db.BookingPassengers
                .Where(p => p.Booking.ScheduleId == dto.ScheduleId && p.Booking.Status == BookingStatus.Confirmed)
                .Select(p => p.SeatNumber)
                .ToListAsync();

            var activeBlocks = await _db.SeatBlocks
                .Where(sb => sb.ScheduleId == dto.ScheduleId && !sb.IsReleased && sb.ExpiresAt > now)
                .Select(sb => sb.SeatNumber)
                .ToListAsync();

            var conflict = dto.SeatNumbers.FirstOrDefault(s => bookedSeats.Contains(s) || activeBlocks.Contains(s));
            if (conflict != null)
                return new SeatBlockResponseDto { Success = false, Message = $"Seat {conflict} is already taken." };

            // Release any previous blocks by this user for this schedule
            var myOldBlocks = await _db.SeatBlocks
                .Where(sb => sb.ScheduleId == dto.ScheduleId && sb.BlockedByUserId == userId && !sb.IsReleased)
                .ToListAsync();
            myOldBlocks.ForEach(b => b.IsReleased = true);

            var expiresAt = now.AddMinutes(5);
            foreach (var seat in dto.SeatNumbers)
            {
                _db.SeatBlocks.Add(new SeatBlock
                {
                    ScheduleId = dto.ScheduleId,
                    SeatNumber = seat,
                    BlockedByUserId = userId,
                    BlockedAt = now,
                    ExpiresAt = expiresAt,
                    IsReleased = false
                });
            }

            await _db.SaveChangesAsync();

            return new SeatBlockResponseDto
            {
                Success = true,
                Message = "Seats blocked for 5 minutes.",
                ExpiresAt = expiresAt,
                BlockedSeats = dto.SeatNumbers
            };
        }

        public async Task<BookingResponseDto> CreateBookingAsync(int userId, CreateBookingDto dto)
        {
            var now = DateTime.UtcNow;
            var feePercent = await GetPlatformFeePercentAsync();

            var schedule = await _db.BusSchedules
                .Include(s => s.Bus).ThenInclude(b => b.Layout)
                .Include(s => s.Route)
                .FirstOrDefaultAsync(s => s.Id == dto.ScheduleId && !s.IsCancelled)
                ?? throw new InvalidOperationException("Schedule not found or cancelled.");

            // Verify all seats are still blocked by this user
            var requestedSeats = dto.Passengers.Select(p => p.SeatNumber).ToList();
            var myBlocks = await _db.SeatBlocks
                .Where(sb => sb.ScheduleId == dto.ScheduleId && sb.BlockedByUserId == userId
                             && !sb.IsReleased && sb.ExpiresAt > now
                             && requestedSeats.Contains(sb.SeatNumber))
                .ToListAsync();

            if (myBlocks.Count != requestedSeats.Count)
                throw new InvalidOperationException("Seat block expired. Please reselect your seats.");

            decimal operatorTotal = schedule.PricePerSeat * dto.Passengers.Count;
            decimal platformFee = Math.Round(operatorTotal * feePercent / 100, 2);

            var booking = new Booking
            {
                CustomerId = userId,
                ScheduleId = dto.ScheduleId,
                TotalAmount = operatorTotal,
                PlatformFee = platformFee,
                Status = BookingStatus.Confirmed,
                BookedAt = now
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            foreach (var p in dto.Passengers)
            {
                _db.BookingPassengers.Add(new BookingPassenger
                {
                    BookingId = booking.Id,
                    SeatNumber = p.SeatNumber,
                    PassengerName = p.PassengerName,
                    Age = p.Age,
                    Gender = p.Gender
                });
            }

            // Release blocks
            myBlocks.ForEach(b => b.IsReleased = true);
            await _db.SaveChangesAsync();

            var response = new BookingResponseDto
            {
                BookingId = booking.Id,
                Status = booking.Status.ToString(),
                TotalAmount = operatorTotal,
                PlatformFee = platformFee,
                GrandTotal = operatorTotal + platformFee,
                BusName = schedule.Bus.BusName,
                Source = schedule.Route.SourceCity,
                Destination = schedule.Route.DestinationCity,
                DepartureTime = schedule.DepartureTime,
                BookedAt = booking.BookedAt,
                Passengers = dto.Passengers
            };

            // Fire-and-forget email notification
            var customer = await _db.Users.FindAsync(userId);
            if (customer != null)
            {
                _ = _email.SendBookingConfirmationAsync(customer.Email, customer.Name, new BookingEmailDto
                {
                    BookingId     = booking.Id,
                    BusName       = schedule.Bus.BusName,
                    BusType       = schedule.Bus.BusType,
                    OperatorName  = schedule.Bus.Operator?.User?.Name ?? "Operator",
                    Source        = schedule.Route.SourceCity,
                    Destination   = schedule.Route.DestinationCity,
                    DepartureTime = schedule.DepartureTime,
                    ArrivalTime   = schedule.ArrivalTime,
                    BaseAmount    = operatorTotal,
                    PlatformFee   = platformFee,
                    GrandTotal    = operatorTotal + platformFee,
                    SeatNumbers   = dto.Passengers.Select(p => p.SeatNumber).ToList(),
                    Passengers    = dto.Passengers.Select(p => new PassengerInfo
                    {
                        SeatNumber = p.SeatNumber,
                        Name       = p.PassengerName,
                        Age        = p.Age,
                        Gender     = p.Gender
                    }).ToList()
                });
            }

            return response;
        }

        public async Task<PaymentResponseDto> ProcessPaymentAsync(int userId, PaymentRequestDto dto)
        {
            var booking = await _db.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == dto.BookingId && b.CustomerId == userId)
                ?? throw new InvalidOperationException("Booking not found.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Cannot pay for a cancelled booking.");

            if (booking.Payment?.Status == PaymentStatus.Success)
                return new PaymentResponseDto { Success = true, Message = "Already paid.", TransactionId = booking.Payment.TransactionId, Amount = booking.Payment.Amount };

            var transactionId = $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
            var total = booking.TotalAmount + booking.PlatformFee;

            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = total,
                PaymentMethod = dto.PaymentMethod,
                TransactionId = transactionId,
                Status = PaymentStatus.Success,
                PaidAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            return new PaymentResponseDto
            {
                Success = true,
                TransactionId = transactionId,
                Amount = total,
                Message = "Payment successful!"
            };
        }

        public async Task<List<BookingListDto>> GetMyBookingsAsync(int userId)
        {
            return await _db.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Route)
                .Include(b => b.Schedule).ThenInclude(s => s.Bus).ThenInclude(bus => bus.Operator).ThenInclude(o => o.User)
                .Include(b => b.Passengers)
                .Where(b => b.CustomerId == userId)
                .OrderByDescending(b => b.BookedAt)
                .Select(b => new BookingListDto
                {
                    BookingId = b.Id,
                    BusName = b.Schedule.Bus.BusName,
                    OperatorName = b.Schedule.Bus.Operator.User.Name,
                    Source = b.Schedule.Route.SourceCity,
                    Destination = b.Schedule.Route.DestinationCity,
                    DepartureTime = b.Schedule.DepartureTime,
                    BookedAt = b.BookedAt,
                    TotalAmount = b.TotalAmount + b.PlatformFee,
                    Status = b.Status.ToString(),
                    RefundAmount = b.RefundAmount,
                    PassengerCount = b.Passengers.Count,
                    SeatNumbers = b.Passengers.Select(p => p.SeatNumber).ToList()
                })
                .ToListAsync();
        }

        public async Task<BookingResponseDto> GetBookingDetailAsync(int userId, int bookingId)
        {
            var b = await _db.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Route)
                .Include(b => b.Schedule).ThenInclude(s => s.Bus)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId)
                ?? throw new InvalidOperationException("Booking not found.");

            return new BookingResponseDto
            {
                BookingId = b.Id,
                Status = b.Status.ToString(),
                TotalAmount = b.TotalAmount,
                PlatformFee = b.PlatformFee,
                GrandTotal = b.TotalAmount + b.PlatformFee,
                BusName = b.Schedule.Bus.BusName,
                Source = b.Schedule.Route.SourceCity,
                Destination = b.Schedule.Route.DestinationCity,
                DepartureTime = b.Schedule.DepartureTime,
                BookedAt = b.BookedAt,
                Passengers = b.Passengers.Select(p => new PassengerDto
                {
                    SeatNumber = p.SeatNumber,
                    PassengerName = p.PassengerName,
                    Age = p.Age,
                    Gender = p.Gender
                }).ToList()
            };
        }

        public async Task<CancelResponseDto> CancelBookingAsync(int userId, int bookingId, CancelBookingDto dto)
        {
            var booking = await _db.Bookings
                .Include(b => b.Schedule)
                .Include(b => b.Payment)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId)
                ?? throw new InvalidOperationException("Booking not found.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking is already cancelled.");

            // Calculate refund based on policy
            var hoursBeforeDeparture = (booking.Schedule.DepartureTime - DateTime.UtcNow).TotalHours;
            decimal refundPercent = 0;
            string policy = "";

            var settings = await _db.PlatformSettings
                .Where(s => s.Key.StartsWith("RefundPolicy_"))
                .ToDictionaryAsync(s => s.Key, s => decimal.Parse(s.Value));

            if (hoursBeforeDeparture > 24)
            {
                refundPercent = settings.GetValueOrDefault("RefundPolicy_24hr", 80);
                policy = $"Cancelled > 24 hours before departure: {refundPercent}% refund";
            }
            else if (hoursBeforeDeparture > 12)
            {
                refundPercent = settings.GetValueOrDefault("RefundPolicy_12hr", 50);
                policy = $"Cancelled 12–24 hours before departure: {refundPercent}% refund";
            }
            else
            {
                refundPercent = settings.GetValueOrDefault("RefundPolicy_0hr", 0);
                policy = $"Cancelled < 12 hours before departure: {refundPercent}% refund";
            }

            decimal totalPaid = booking.TotalAmount + booking.PlatformFee;
            decimal refundAmount = Math.Round(totalPaid * refundPercent / 100, 2);

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
                RefundPolicy = policy
            };

            // Fire-and-forget cancellation email
            var customer = await _db.Users.FindAsync(userId);
            if (customer != null)
            {
                _ = _email.SendBookingCancellationAsync(customer.Email, customer.Name, new CancellationEmailDto
                {
                    BookingId = booking.Id,
                    BusName = booking.Schedule.Bus?.BusName ?? "Bus",
                    Source = booking.Schedule.Route?.SourceCity ?? string.Empty,
                    Destination = booking.Schedule.Route?.DestinationCity ?? string.Empty,
                    RefundAmount = refundAmount,
                    RefundPolicy = policy
                });
            }

            return cancelResponse;
        }

        public async Task<ProfileDto> GetProfileAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            return new ProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<ProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            user.Name = dto.Name;
            user.Phone = dto.Phone;
            await _db.SaveChangesAsync();

            return new ProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}
