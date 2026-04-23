namespace BusBookingSystem.Models
{
    public class BookingPassenger
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;

        public Booking Booking { get; set; } = null!;
    }
}
