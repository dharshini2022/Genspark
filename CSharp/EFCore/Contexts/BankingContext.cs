using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore.Models;

namespace EFCore.Contexts
{
    public class BankingContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=bankDB;Username=postgres;Password=12345");
        }

        public DbSet<Customer>  customers { get; set; }
        public DbSet<Account> accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(c =>
            {
                c.HasKey(c => c.Id);
                c.Property(c => c.DateOfBirth).HasColumnType("timestamp without time zone");
                //seeding (data added during table creation)
                c.HasData(new Customer() { Id = 101, Name = "Ramu", DateOfBirth = new DateTime(2002,05,13), Phone = "9876543210", Email = "ramu@gmail.com", Status = "Active" });
            });



            modelBuilder.Entity<Account>(a =>
            {
                a.HasKey(a => a.AccountNumber);

                a.HasOne(a => a.Customer)       //1 account has 1 customer
                .WithMany(c => c.Accounts)     //1 Customer has many accounts
                .HasForeignKey(a => a.CustomerId)   //FK at accounts table
                .HasConstraintName("FK_Account_Customer")   //FK constraint name
                .OnDelete(DeleteBehavior.Restrict);     //restricts deletion at parent & child table

                a.Property(a => a.LastAccessed).HasColumnType("timestamp without time zone");   //define datatype of a clmn

                //seeding
                a.HasData(new Account()
                {
                    AccountNumber = "0009998877",
                    Balance = 134.3M,
                    CustomerId = 101,
                    LastAccessed = new DateTime(2026,5,13),
                    Status = "Active"
                });
            });

        }
    }
}