using System.ComponentModel.DataAnnotations;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
        {
            
        }
        public DbSet<Member> Members {get; set;}
        public DbSet<Book> Books {get;set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>(m =>
            {
                m.HasKey(m => m.MemberId).HasName("PK_MemberId");
                m.HasIndex(m => m.Email).IsUnique();
                m.HasIndex(m => m.PhoneNumber).IsUnique();
                m.Property(m => m.IsActive).HasDefaultValueSql("true");
                m.Property(m => m.MembershipDate).HasColumnType("timestamp without time zone");
                //seed Data
                m.HasData(new Member(){ MemberId = 1, FullName = "Dharshini K", Email = "dharshini@gmail.com", PhoneNumber = "9876543210", MembershipDate = new DateTime(2026,01,01), IsActive = true});
            });

            modelBuilder.Entity<Book>(b =>
            {
                b.HasKey(b => b.BookId).HasName("PK_BookId");
                b.HasIndex(b => b.Title).IsUnique();
                b.Property(b => b.Title).IsRequired();
                b.HasIndex(b => b.Author);
                b.Property(b => b.Author).IsRequired();
                b.Property(b => b.AvailableCopies).HasDefaultValue(1);
                //seed Data
                b.HasData(new Book(){ BookId = 1, Title = "Seed Data", Author = "Seed author", ISBN = "ISBN409", PublishedYear = 2020, AvailableCopies = 3});
            });
        }
    }
}