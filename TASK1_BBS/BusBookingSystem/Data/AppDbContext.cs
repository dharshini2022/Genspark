using Microsoft.EntityFrameworkCore;
using BusBookingSystem.Models;

namespace BusBookingSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<BusOperator> BusOperators => Set<BusOperator>();
        public DbSet<OperatorOffice> OperatorOffices => Set<OperatorOffice>();
        public DbSet<BusRoute> Routes => Set<BusRoute>();
        public DbSet<BusLayout> BusLayouts => Set<BusLayout>();
        public DbSet<Bus> Buses => Set<Bus>();
        public DbSet<BusSchedule> BusSchedules => Set<BusSchedule>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingPassenger> BookingPassengers => Set<BookingPassenger>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<SeatBlock> SeatBlocks => Set<SeatBlock>();
        public DbSet<PlatformSetting> PlatformSettings => Set<PlatformSetting>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Bus>().HasIndex(b => b.RegistrationNumber).IsUnique();
            modelBuilder.Entity<PlatformSetting>().HasIndex(p => p.Key).IsUnique();

            // Decimal precision
            modelBuilder.Entity<Booking>().Property(b => b.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Booking>().Property(b => b.PlatformFee).HasPrecision(18, 2);
            modelBuilder.Entity<Booking>().Property(b => b.RefundAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<BusSchedule>().Property(s => s.PricePerSeat).HasPrecision(18, 2);

            // Cascade rules
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BusOperator>()
                .HasOne(o => o.User)
                .WithOne(u => u.OperatorProfile)
                .HasForeignKey<BusOperator>(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            var adminPasswordHash  = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            var admin2PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "System Admin",
                    Email = "admin@busbooking.com",
                    Phone = "9999999999",
                    PasswordHash = adminPasswordHash,
                    Role = UserRole.Admin,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new User
                {
                    Id = 7,
                    Name = "Dharshini K",
                    Email = "dharshini.k2022cce@sece.ac.in",
                    Phone = "9000000001",
                    PasswordHash = admin2PasswordHash,
                    Role = UserRole.Admin,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                }
            );

            modelBuilder.Entity<PlatformSetting>().HasData(
                new PlatformSetting
                {
                    Id = 1,
                    Key = "PlatformFeePercent",
                    Value = "5",
                    Description = "Platform fee percentage added on top of operator price",
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedByAdminId = 1
                },
                new PlatformSetting
                {
                    Id = 2,
                    Key = "RefundPolicy_24hr",
                    Value = "80",
                    Description = "Refund % if cancelled more than 24 hours before departure",
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedByAdminId = 1
                },
                new PlatformSetting
                {
                    Id = 3,
                    Key = "RefundPolicy_12hr",
                    Value = "50",
                    Description = "Refund % if cancelled between 12 and 24 hours before departure",
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedByAdminId = 1
                },
                new PlatformSetting
                {
                    Id = 4,
                    Key = "RefundPolicy_0hr",
                    Value = "0",
                    Description = "Refund % if cancelled less than 12 hours before departure",
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedByAdminId = 1
                }
            );

            modelBuilder.Entity<BusRoute>().HasData(
                new BusRoute
                {
                    Id = 1,
                    SourceCity = "Chennai",
                    DestinationCity = "Coimbatore",
                    CreatedByAdminId = 1,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new BusRoute
                {
                    Id = 2,
                    SourceCity = "Chennai",
                    DestinationCity = "Madurai",
                    CreatedByAdminId = 1,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                }
            );
            modelBuilder.Entity<BusLayout>().HasData(
                new BusLayout
                {
                    Id = 1,
                    Name = "Standard 2+2 (40 Seats)",
                    Description = "10 rows, 2 seats on each side, 40 total seats",
                    TotalRows = 10,
                    SeatsPerRow = 4,
                    HasUpperDeck = false,
                    IsGlobal = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LayoutJson = "{\"rows\":10,\"cols\":4,\"aisle\":1,\"seats\":[[\"1A\",\"1B\",null,\"1C\",\"1D\"],[\"2A\",\"2B\",null,\"2C\",\"2D\"],[\"3A\",\"3B\",null,\"3C\",\"3D\"],[\"4A\",\"4B\",null,\"4C\",\"4D\"],[\"5A\",\"5B\",null,\"5C\",\"5D\"],[\"6A\",\"6B\",null,\"6C\",\"6D\"],[\"7A\",\"7B\",null,\"7C\",\"7D\"],[\"8A\",\"8B\",null,\"8C\",\"8D\"],[\"9A\",\"9B\",null,\"9C\",\"9D\"],[\"10A\",\"10B\",null,\"10C\",\"10D\"]]}"
                },
                new BusLayout
                {
                    Id = 2,
                    Name = "Sleeper 2+1 (30 Berths)",
                    Description = "10 rows, 2 berths on left, 1 berth on right",
                    TotalRows = 10,
                    SeatsPerRow = 3,
                    HasUpperDeck = false,
                    IsGlobal = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LayoutJson = "{\"rows\":10,\"cols\":3,\"aisle\":1,\"seats\":[[\"1A\",\"1B\",null,\"1C\"],[\"2A\",\"2B\",null,\"2C\"],[\"3A\",\"3B\",null,\"3C\"],[\"4A\",\"4B\",null,\"4C\"],[\"5A\",\"5B\",null,\"5C\"],[\"6A\",\"6B\",null,\"6C\"],[\"7A\",\"7B\",null,\"7C\"],[\"8A\",\"8B\",null,\"8C\"],[\"9A\",\"9B\",null,\"9C\"],[\"10A\",\"10B\",null,\"10C\"]]}"
                }
            );
        }
    }
}
