namespace BusBookingSystem.Models
{
    public class OperatorOffice
    {
        public int Id { get; set; }
        public int OperatorId { get; set; }
        public string District { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public BusOperator Operator { get; set; } = null!;
    }
}
