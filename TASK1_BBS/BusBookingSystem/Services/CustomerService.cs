using BusBookingSystem.Data;
using BusBookingSystem.DTOs;
using BusBookingSystem.Interfaces;
using BusBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Services
{
    // OOP — Abstraction: ICustomerService interface moved to Interfaces/ICustomerService.cs
    // OOP — Encapsulation: LayoutConfig class moved to Models/LayoutConfig.cs as internal sealed

    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private static readonly string[] DefaultPhotos = { "10fb39a7-8c38-46ba-bdb8-181724599a06.jpg", "18168f51-1d29-4440-a1e5-e6cc91e574c9.webp" };

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

        public async Task<BusSearchResultDto> GetScheduleDetailsAsync(int scheduleId)
        {
            var s = await _db.BusSchedules
                .Include(s => s.Bus).ThenInclude(b => b.Operator).ThenInclude(o => o.Offices)
                .Include(s => s.Bus).ThenInclude(b => b.Operator).ThenInclude(o => o.User)
                .Include(s => s.Bus).ThenInclude(b => b.Layout)
                .Include(s => s.Route)
                .FirstOrDefaultAsync(sch => sch.Id == scheduleId)
                ?? throw new InvalidOperationException("Schedule not found.");

            return await MapToSearchResultAsync(s);
        }

        private async Task<BusSearchResultDto> MapToSearchResultAsync(BusSchedule s)
        {
            var now = DateTime.UtcNow;
            var passengers = await _db.BookingPassengers
                .Where(p => p.Booking.ScheduleId == s.Id && p.Booking.Status == BookingStatus.Confirmed)
                .ToListAsync();

            var bookedSeats = passengers.Select(p => p.SeatNumber).ToList();

            var blockedSeats = await _db.SeatBlocks
                .Where(sb => sb.ScheduleId == s.Id && !sb.IsReleased && sb.ExpiresAt > now)
                .Select(sb => sb.SeatNumber)
                .ToListAsync();

            // Women-specific detailing logic
            var femaleBooked = new List<string>();
            var womenOnly = new List<string>();

            if (!string.IsNullOrEmpty(s.Bus.Layout?.LayoutJson))
            {
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var layout = System.Text.Json.JsonSerializer.Deserialize<LayoutConfig>(s.Bus.Layout.LayoutJson, options);
                    if (layout?.Seats != null)
                    {
                        foreach (var p in passengers.Where(p => string.Equals(p.Gender, "Female", StringComparison.OrdinalIgnoreCase)))
                        {
                            femaleBooked.Add(p.SeatNumber);

                            // Find adjacent seat
                            string? adjacentSeat = null;
                            for (int r = 0; r < layout.Seats.Length; r++)
                            {
                                for (int c = 0; c < layout.Seats[r].Length; c++)
                                {
                                    if (layout.Seats[r][c] == p.SeatNumber)
                                    {
                                        // Check left and right
                                        if (c > 0 && layout.Seats[r][c - 1] != null) adjacentSeat = layout.Seats[r][c - 1];
                                        else if (c < layout.Seats[r].Length - 1 && layout.Seats[r][c + 1] != null) adjacentSeat = layout.Seats[r][c + 1];
                                        break;
                                    }
                                }
                                if (adjacentSeat != null) break;
                            }

                            if (adjacentSeat != null && !bookedSeats.Contains(adjacentSeat) && !blockedSeats.Contains(adjacentSeat))
                            {
                                var otherInSameBooking = passengers.Any(op => op.BookingId == p.BookingId && op.SeatNumber == adjacentSeat);
                                if (!otherInSameBooking)
                                {
                                    womenOnly.Add(adjacentSeat);
                                }
                            }
                        }
                    }
                }
                catch { /* Layout parsing failed */ }
            }

            var uniqueBooked = bookedSeats.Distinct().ToList();
            var uniqueBlocked = blockedSeats.Distinct().ToList();
            int totalSeats = s.Bus.Layout?.TotalRows * s.Bus.Layout?.SeatsPerRow ?? 40;
            int available = totalSeats - uniqueBooked.Count - uniqueBlocked.Count;

            decimal platformFee = Math.Round(s.PricePerSeat * 0.05m, 2);
            decimal gst = Math.Round(s.PricePerSeat * 0.02m, 2);

            return new BusSearchResultDto
            {
                ScheduleId = s.Id,
                BusId = s.BusId,
                BusName = s.Bus.BusName,
                BusType = s.Bus.BusType,
                OperatorName = s.Bus.Operator.CompanyName,
                OperatorPhone = s.Bus.Operator.User.Phone,
                Source = s.Route.SourceCity,
                Destination = s.Route.DestinationCity,
                DepartureTime = s.DepartureTime,
                ArrivalTime = s.ArrivalTime,
                PricePerSeat = s.PricePerSeat,
                PlatformFee = platformFee,
                Gst = gst,
                TotalPrice = s.PricePerSeat + platformFee + gst,
                BoardingAddress = s.Bus.Operator.Offices.FirstOrDefault(o => o.City.Trim().Equals(s.Route.SourceCity.Trim(), StringComparison.OrdinalIgnoreCase))?.Address ?? $"{s.Route.SourceCity} Bus Stand",
                DroppingAddress = s.Bus.Operator.Offices.FirstOrDefault(o => o.City.Trim().Equals(s.Route.DestinationCity.Trim(), StringComparison.OrdinalIgnoreCase))?.Address ?? $"{s.Route.DestinationCity} Bus Stand",
                AvailableSeats = Math.Max(0, available),
                TotalSeats = totalSeats,
                LayoutJson = s.Bus.Layout?.LayoutJson ?? string.Empty,
                BookedSeats = uniqueBooked,
                BlockedSeats = uniqueBlocked,
                FemaleBookedSeats = femaleBooked.Distinct().ToList(),
                WomenOnlySeats = womenOnly.Distinct().ToList(),
                Features = s.Bus.Features != null ? s.Bus.Features.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new(),
                Photos = (string.IsNullOrWhiteSpace(s.Bus.Photos) ? DefaultPhotos : s.Bus.Photos.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(p => p.StartsWith("/") ? p : "/img/" + p).ToList()
            };
        }

        public async Task<List<BusSearchResultDto>> SearchBusesAsync(string source, string destination, DateOnly date)
        {
            var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            var schedules = await _db.BusSchedules
                .Include(s => s.Bus).ThenInclude(b => b.Operator).ThenInclude(o => o.Offices)
                .Include(s => s.Bus).ThenInclude(b => b.Operator).ThenInclude(o => o.User)
                .Include(s => s.Bus).ThenInclude(b => b.Layout)
                .Include(s => s.Route)
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
                result.Add(await MapToSearchResultAsync(s));
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
                .Where(sb => sb.ScheduleId == dto.ScheduleId && !sb.IsReleased && sb.ExpiresAt > now && sb.BlockedByUserId != userId)
                .Select(sb => sb.SeatNumber)
                .ToListAsync();

            var conflict = dto.SeatNumbers.FirstOrDefault(s => bookedSeats.Contains(s) || activeBlocks.Contains(s));
            if (conflict != null)
                return new SeatBlockResponseDto { Success = false, Message = $"Seat {conflict} is already taken by another user." };

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

            var schedule = await _db.BusSchedules
                .Include(s => s.Bus).ThenInclude(b => b.Layout)
                .Include(s => s.Bus).ThenInclude(b => b.Operator).ThenInclude(o => o.Offices)
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
            decimal platformFee = Math.Round(operatorTotal * 0.05m, 2);
            decimal gst = Math.Round(operatorTotal * 0.02m, 2);

            var booking = new Booking
            {
                CustomerId = userId,
                ScheduleId = dto.ScheduleId,
                TotalAmount = operatorTotal,
                PlatformFee = platformFee,
                Gst = gst,
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

            // Find boarding/dropping addresses
            var boardingOffice = schedule.Bus.Operator.Offices.FirstOrDefault(o => o.City.ToLower() == schedule.Route.SourceCity.ToLower());
            var droppingOffice = schedule.Bus.Operator.Offices.FirstOrDefault(o => o.City.ToLower() == schedule.Route.DestinationCity.ToLower());
            string boardingAddress = boardingOffice?.Address ?? $"{schedule.Route.SourceCity} Bus Stand";
            string droppingAddress = droppingOffice?.Address ?? $"{schedule.Route.DestinationCity} Bus Stand";

            var response = new BookingResponseDto
            {
                BookingId = booking.Id,
                Status = booking.Status.ToString(),
                TotalAmount = operatorTotal,
                PlatformFee = platformFee,
                Gst = gst,
                GrandTotal = operatorTotal + platformFee + gst,
                BusName = schedule.Bus.BusName,
                Source = schedule.Route.SourceCity,
                Destination = schedule.Route.DestinationCity,
                BoardingAddress = boardingAddress,
                DroppingAddress = droppingAddress,
                DepartureTime = schedule.DepartureTime,
                BookedAt = booking.BookedAt,
                Passengers = dto.Passengers
            };

            // Await email notification to ensure it's sent
            var customer = await _db.Users.FindAsync(userId);
            if (customer != null)
            {
                try
                {
                    await _email.SendBookingConfirmationAsync(customer.Email, customer.Name, new BookingEmailDto
                    {
                        BookingId     = booking.Id,
                        BusName       = schedule.Bus.BusName,
                        BusType       = schedule.Bus.BusType,
                        OperatorName  = schedule.Bus.Operator?.User?.Name ?? "Operator",
                        Source        = schedule.Route.SourceCity,
                        Destination   = schedule.Route.DestinationCity,
                        BoardingAddress = boardingAddress,
                        DroppingAddress = droppingAddress,
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
                catch (Exception ex)
                {
                    // Log but don't fail the booking if email fails
                    // The email service already has a try-catch, but extra safety here
                }
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
            var total = booking.TotalAmount + booking.PlatformFee + booking.Gst;

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
                    TotalAmount = b.TotalAmount,
                    PlatformFee = b.PlatformFee,
                    Gst = b.Gst,
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
                .Include(b => b.Schedule).ThenInclude(s => s.Bus).ThenInclude(bus => bus.Operator).ThenInclude(o => o.Offices)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId)
                ?? throw new InvalidOperationException("Booking not found.");

            var boardingOffice = b.Schedule.Bus.Operator.Offices.FirstOrDefault(o => o.City.ToLower() == b.Schedule.Route.SourceCity.ToLower());
            var droppingOffice = b.Schedule.Bus.Operator.Offices.FirstOrDefault(o => o.City.ToLower() == b.Schedule.Route.DestinationCity.ToLower());
            string boardingAddress = boardingOffice?.Address ?? $"{b.Schedule.Route.SourceCity} Bus Stand";
            string droppingAddress = droppingOffice?.Address ?? $"{b.Schedule.Route.DestinationCity} Bus Stand";

            return new BookingResponseDto
            {
                BookingId = b.Id,
                Status = b.Status.ToString(),
                TotalAmount = b.TotalAmount,
                PlatformFee = b.PlatformFee,
                Gst = b.Gst,
                GrandTotal = b.TotalAmount + b.PlatformFee + b.Gst,
                BusName = b.Schedule.Bus.BusName,
                Source = b.Schedule.Route.SourceCity,
                Destination = b.Schedule.Route.DestinationCity,
                BoardingAddress = boardingAddress,
                DroppingAddress = droppingAddress,
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

            decimal totalPaid = booking.TotalAmount + booking.PlatformFee + booking.Gst;
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

            // Await cancellation email
            var customer = await _db.Users.FindAsync(userId);
            if (customer != null)
            {
                try
                {
                    await _email.SendBookingCancellationAsync(customer.Email, customer.Name, new CancellationEmailDto
                    {
                        BookingId = booking.Id,
                        BusName = booking.Schedule.Bus?.BusName ?? "Bus",
                        Source = booking.Schedule.Route?.SourceCity ?? string.Empty,
                        Destination = booking.Schedule.Route?.DestinationCity ?? string.Empty,
                        RefundAmount = refundAmount,
                        RefundPolicy = policy
                    });
                }
                catch { }
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
