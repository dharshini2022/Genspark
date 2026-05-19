using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.DAL.Context;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public LibraryDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=LibrarySystem;Username=postgres;Password=12345; Include Error Detail = true;");
    }
    public DbSet<Member> Members {get; set; }

    public DbSet<Book> Books {get; set;}

    public DbSet<BookCategory> BookCategories {get; set;}

    public DbSet<BookCopy> BookCopies {get; set;}

    public DbSet<Borrowing> Borrowings {get; set;}

    public DbSet<Fine> Fines {get; set;}
    public DbSet<DamageLog> DamageLogs {get; set;}


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Member
        modelBuilder.Entity<Member>()
            .HasKey(m => m.MemberId);

        modelBuilder.Entity<Member>()
            .HasIndex(m => m.Email)
            .IsUnique();

        modelBuilder.Entity<Member>()
            .Property(m => m.JoinedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");

        //BookCategory
        modelBuilder.Entity<BookCategory>()
            .HasKey(bc => bc.CategoryId);

        // Relationship: BookCategory to Book (One to Many) (Plural : Collection of Objects)
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(bc => bc.Books)
            .HasForeignKey(b => b.CategoryId);

        //Book
        modelBuilder.Entity<Book>()
            .HasKey(b => b.BookId);

        modelBuilder.Entity<Book>()
            .HasIndex(b => b.ISBN)
            .IsUnique();

        //BookCopy
        modelBuilder.Entity<BookCopy>()
            .HasKey(bc => bc.CopyId);

        modelBuilder.Entity<BookCopy>()
            .Property(bc => bc.CopyId)
            .UseIdentityByDefaultColumn();

        modelBuilder.Entity<BookCopy>()
            .HasIndex(bc => bc.CopyCode)
            .IsUnique();

        // Relationship: Book to BookCopy (One to Many)
        modelBuilder.Entity<BookCopy>()
            .HasOne(bc => bc.Book)
            .WithMany(b => b.Copies)
            .HasForeignKey(bc => bc.BookId);

        //Borrowing
        modelBuilder.Entity<Borrowing>()
            .HasKey(br => br.BorrowingId);
        
        modelBuilder.Entity<Borrowing>()
            .Property(m => m.BorrowedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");

        modelBuilder.Entity<Borrowing>()
            .Property(m => m.ReturnedAt)
            .HasColumnType("timestamp without time zone");

        modelBuilder.Entity<Borrowing>()
            .Property(m => m.DueDate)
            .HasColumnType("timestamp without time zone");
        

        //Relationship: Member to Borrowing (One to Many)
        modelBuilder.Entity<Borrowing>()
            .HasOne(br => br.Member)
            .WithMany(m => m.Borrowings)
            .HasForeignKey(br => br.MemberId);

        //Relationship: BookCopy to Borrowing (One to Many)
        modelBuilder.Entity<Borrowing>()
            .HasOne(br => br.Copy)
            .WithMany(bc => bc.Borrowings)
            .HasForeignKey(br => br.CopyId);

        //Fine
        modelBuilder.Entity<Fine>()
            .HasKey(f => f.FineId);

        modelBuilder.Entity<Fine>()
            .Property(m => m.PaidAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");

        // Relationship: Member to Fine (One to Many)
        modelBuilder.Entity<Fine>()
            .HasOne(f => f.Member)
            .WithMany(m => m.FinePayments)
            .HasForeignKey(f => f.MemberId);

        //Relationship : Borrowing to Fine (One to One) (Singular single object)
        modelBuilder.Entity<Fine>()
            .HasOne(f => f.Borrowing)
            .WithOne(br => br.FinePayment)
            .HasForeignKey<Fine>(f => f.BorrowingId);

        //Damage Logs
        modelBuilder.Entity<DamageLog>()
            .HasKey(d => d.DamageId);

        modelBuilder.Entity<DamageLog>()
            .Property(d => d.DateOfEntry)
            .HasColumnType("Date");

        modelBuilder.Entity<DamageLog>()
            .Property(d => d.Description)
            .HasColumnType("text");

        modelBuilder.Entity<DamageLog>()
            .Property(m => m.DateOfEntry)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp without time zone");

        //Relationship: Member to DamageLogs (1 to many)
        modelBuilder.Entity<DamageLog>()
            .HasOne(d => d.Member)
            .WithMany(m => m.DamageLogs)
            .HasForeignKey(d => d.MemberId);

        //Relationship: BookCopy to DamageLogs (1 to many)
        modelBuilder.Entity<DamageLog>()
            .HasOne(d => d.BookCopy)
            .WithMany(b => b.DamageLogs)
            .HasForeignKey(d => d.CopyId);
    }
}