// using LibrarySystem.Business.Services;
using LibrarySystem.BLL.Services;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.Presentation.Menus;
using LibrarySystem.UI.Session;

namespace LibrarySystem.UI.Menus
{
    public  class AuthMenu
    {
        private  readonly AuthService _authService;
        private readonly AdminMenu _adminMenu;
        private readonly MemberMenu _memberMenu;

        public AuthMenu(AuthService authService, AdminMenu adminMenu, MemberMenu memberMenu)
        {
            _authService = authService;
            _adminMenu = adminMenu;
            _memberMenu = memberMenu;
        }

        public void Register()
        {
            Console.WriteLine("====== REGISTER ======");
            Member member = new Member();
            Console.Write("Name : ");
            member.Name = (Console.ReadLine() ?? "").Trim().ToLower();

            Console.Write("Email : ");
            member.Email = (Console.ReadLine() ?? "").Trim().ToLower();

            Console.Write("Password : ");
            member.Password = (Console.ReadLine() ?? "").Trim();

            _authService.Register(member);

            Console.WriteLine("Registration Successful");
            Console.WriteLine("Please Login...");
        }

        public void Login()
        {
            Console.WriteLine("====== LOGIN ======");
            Console.Write("Email : ");
            string email = (Console.ReadLine() ?? "").Trim().ToLower();

            Console.Write("Password : ");
            string password = (Console.ReadLine() ?? "").Trim().ToLower();

            var user = _authService.Login(email, password);

            if (user == null)
            {
                throw new InvalidMemberException("Invalid Creds!");
            }
            SessionManager.Login(user);

            if (user.Role == Member.memberRole.Admin)
            {
                _adminMenu.Show();
            }
            else
            {
                _memberMenu.Show();
            }
        }
    }
}