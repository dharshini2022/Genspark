using CFA.Models;
using Microsoft.EntityFrameworkCore;
namespace CFA.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        //1.Model to Table (Mapping)
        public DbSet<Department> Departments { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }

        //2.Connection String
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     if (!optionsBuilder.IsConfigured)
        //     {
        //         optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=efcore_db;Username=postgres;Password=12345");
        //     }
        // }

        //3. Model Property spec using FluentAPI
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Student Table
            modelBuilder.Entity<Student>(s =>
            {
                s.HasKey(s => s.StudentId);
                s.Property(s => s.Name).IsRequired().HasMaxLength(100);
                s.Property(s => s.DateOfBirth).HasColumnType("timestamp without time zone");

            });
            //Department Table
            modelBuilder.Entity<Department>(d =>{
                d.HasKey(d => d.DepartmentId);
                d.Property(d => d.Name).IsRequired().HasMaxLength(100);
                //seed data
                d.HasData(new Department(){ DepartmentId = 1, Name = "Computer Science" });
            });   

            //Relationships         
            // One-to-Many (1 department -> Many students)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Students)
                .HasForeignKey(s => s.DepartmentId);
            // One-to-One (1 Profile -> 1 Student)
            modelBuilder.Entity<StudentProfile>()
                .HasOne(p => p.Student)
                .WithOne(s => s.StudentProfile)
                .HasForeignKey<StudentProfile>(p => p.StudentId);
        }
    }
}