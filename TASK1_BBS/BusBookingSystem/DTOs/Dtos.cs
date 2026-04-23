namespace BusBookingSystem.DTOs
{
    // ── AUTH ──────────────────────────────────────────────────────────────────

    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool AcceptedTerms { get; set; }
    }

    public class OperatorRegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public List<OfficeDto> Offices { get; set; } = new();
    }

    public class OfficeDto
    {
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    // ── SEARCH ───────────────────────────────────────────────────────────────

    public class BusSearchResultDto
    {
        public int ScheduleId { get; set; }
        public int BusId { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string BusType { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal PricePerSeat { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TotalPrice { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalSeats { get; set; }
        public string LayoutJson { get; set; } = string.Empty;
        public List<string> BookedSeats { get; set; } = new();
        public List<string> BlockedSeats { get; set; } = new();
    }

    // ── SEAT BLOCK ────────────────────────────────────────────────────────────

    public class SeatBlockRequestDto
    {
        public int ScheduleId { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
    }

    public class SeatBlockResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public List<string> BlockedSeats { get; set; } = new();
    }

    // ── BOOKING ───────────────────────────────────────────────────────────────

    public class CreateBookingDto
    {
        public int ScheduleId { get; set; }
        public List<PassengerDto> Passengers { get; set; } = new();
    }

    public class PassengerDto
    {
        public string SeatNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
    }

    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal GrandTotal { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime BookedAt { get; set; }
        public List<PassengerDto> Passengers { get; set; } = new();
    }

    public class BookingListDto
    {
        public int BookingId { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;  // operator company name (for customer view)
        public string CustomerName { get; set; } = string.Empty;  // passenger name (for operator/admin view)
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime BookedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? RefundAmount { get; set; }
        public int PassengerCount { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
    }

    public class CancelBookingDto
    {
        public string? Reason { get; set; }
    }

    public class CancelResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string RefundPolicy { get; set; } = string.Empty;
    }

    // ── PAYMENT ───────────────────────────────────────────────────────────────

    public class PaymentRequestDto
    {
        public int BookingId { get; set; }
        public string PaymentMethod { get; set; } = "Dummy";
        public string? CardLastFour { get; set; }
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ── OPERATOR ──────────────────────────────────────────────────────────────

    public class AddBusDto
    {
        public string BusName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string BusType { get; set; } = string.Empty;
        public int? LayoutId { get; set; }
    }

    public class BusDto
    {
        public int Id { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string BusType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? LayoutName { get; set; }
        public int? LayoutId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateScheduleDto
    {
        public int BusId { get; set; }
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal PricePerSeat { get; set; }
    }

    public class ScheduleDto
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal PricePerSeat { get; set; }
        public bool IsCancelled { get; set; }
        public int TotalBookings { get; set; }
    }

    public class UpdatePriceDto
    {
        public decimal PricePerSeat { get; set; }
    }

    public class UploadLayoutDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int SeatsPerRow { get; set; }
        public bool HasUpperDeck { get; set; }
        public string LayoutJson { get; set; } = string.Empty;
    }

    public class LayoutDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int SeatsPerRow { get; set; }
        public bool HasUpperDeck { get; set; }
        public bool IsGlobal { get; set; }
        public string LayoutJson { get; set; } = string.Empty;
    }

    // ── ADMIN ─────────────────────────────────────────────────────────────────

    public class RouteDto
    {
        public int Id { get; set; }
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalSchedules { get; set; }
    }

    public class CreateRouteDto
    {
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
    }

    public class OperatorListDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public List<OfficeDto> Offices { get; set; } = new();
        public int TotalBuses { get; set; }
    }

    public class BusPendingDto
    {
        public int Id { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string BusType { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RevenueDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalPlatformFee { get; set; }
        public int TotalBookings { get; set; }
        public int TotalCancellations { get; set; }
        public List<OperatorRevenueDto> ByOperator { get; set; } = new();
    }

    public class OperatorRevenueDto
    {
        public int OperatorId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal PlatformFeeCollected { get; set; }
    }

    public class PlatformSettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // ── PROFILE ───────────────────────────────────────────────────────────────

    public class UpdateProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class ProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // ── GENERIC ───────────────────────────────────────────────────────────────

    public class MessageResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
