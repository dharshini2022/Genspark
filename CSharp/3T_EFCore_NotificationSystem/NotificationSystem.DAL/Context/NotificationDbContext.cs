using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotificationSystem.Models;

namespace NotificationSystem.DAL.Contexts
{
    public class NotificationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=NotificationSystem;Username=postgres;Password=12345");
        }

        public DbSet<Notification>  notifications { get; set; }

        public DbSet<User> users { get; set; }

        // public DbSet<SavingAccount> SavingsAccounts { get; set; }

        // public DbSet<CurrentAccount> CurrentAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(c =>
            {
                c.HasKey(c => c.Id).HasName("PK_UserId");
            });


            modelBuilder.Entity<Notification>(n =>
            {
                n.HasKey(n => n.Id)
                .HasName("PK_NotificationId");

                n.Property(n => n.Message)
                 .IsRequired();

                n.Property(n => n.SentDate)
                 .IsRequired();

                n.HasOne(n => n.Sender)
                .WithMany(u => u.SentNotifications)
                .HasForeignKey(n => n.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

                n.HasOne(n => n.Receiver)
                .WithMany(u => u.ReceivedNotifications)
                .HasForeignKey(n => n.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // modelBuilder.Entity<SavingAccount>();
            // modelBuilder.Entity<CurrentAccount>();

        }
    }
}