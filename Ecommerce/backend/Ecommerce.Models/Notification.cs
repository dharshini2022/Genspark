namespace  Ecommerce.Models
{
    public enum NotificationType
    {
        OrderPlaced = 1,
        OrderShipped = 2,
        OrderDelivered = 3,
        PaymentFailed = 4,
        VendorApproved = 5,
        WishlistReminder = 6,
        LowStock = 7,
        OutOfStock = 8,
        VendorPending = 9
    }
    public enum NotificationLevel
    {
        Info = 1,
        Success = 2,
        Warning = 3,
        Error = 4
    }
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public NotificationType Type { get; set; }
        public NotificationLevel Level { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        //Relation
        [System.Text.Json.Serialization.JsonIgnore]
        public User User { get; set; } = null!;
    }
}