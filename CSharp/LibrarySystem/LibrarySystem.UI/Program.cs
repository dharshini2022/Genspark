using LibrarySystem.DAL.Data;
using LibrarySystem.BLL.Interfaces;
using LibrarySystem.BLL.Services;

using LibrarySystem.DAL.Context;
using LibrarySystem.DAL.Interfaces;
using LibrarySystem.DAL.Repositories;

using LibrarySystem.Presentation.Menus;
using LibrarySystem.UI.Menus;

using Microsoft.Extensions.DependencyInjection;

using AdminModules = LibrarySystem.UI.Modules.AdminModule;
using MemberModules = LibrarySystem.UI.Modules.MemberModule;

namespace LibrarySystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddScoped<LibraryDbContext>();

            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IBorrowingRepository, BorrowingRepository>();
            services.AddScoped<IFineRepository, FineRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBookCopyRepository, BookCopyRepository>();
            services.AddScoped<IDamageLogRepository, DamageLogRepository>();

            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IBorrowingService, BorrowingService>();
            services.AddScoped<IFineService, FineService>();
            services.AddScoped<IDamageLogService, DamageLogService>();

            services.AddScoped<AuthService>();

            services.AddScoped<MemberMenu>();
            services.AddScoped<AdminMenu>();
            services.AddScoped<AuthMenu>();

            services.AddScoped<MemberModules.DashboardManagement>();
            services.AddScoped<MemberModules.ProfileManagement>();
            services.AddScoped<MemberModules.BookManagement>();
            services.AddScoped<MemberModules.BorrowManagement>();
            services.AddScoped<MemberModules.FineManagement>();
            services.AddScoped<MemberModules.DamageLogManagement>();

            services.AddScoped<AdminModules.DashboardManagement>();
            services.AddScoped<AdminModules.MemberManagement>();
            services.AddScoped<AdminModules.BookManagement>();
            services.AddScoped<AdminModules.BorrowManagement>();
            services.AddScoped<AdminModules.FineManagement>();
            services.AddScoped<AdminModules.DamageLogManagement>();

            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                DbSeeder.Seed(context);
            }
            var authMenu = serviceProvider.GetRequiredService<AuthMenu>();

            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("====== LIBRARY MANAGEMENT SYSTEM ======");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register");
                Console.WriteLine("3. Exit");
                int choice;

                Console.Write("Enter Choice : ");
                while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 3)
                {
                    Console.Write("Invalid Entry! Enter 1-10: ");
                }
                switch (choice)
                {
                    case 1:
                        authMenu.Login();
                        break;

                    case 2:
                        authMenu.Register();
                        break;

                    case 3:
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Invalid Choice");
                        break;
                }
            }
        }
    }
}