using NotificationSystem.Models;
using NotificationSystem.Interfaces;

namespace NotificationSystem.Services
{
    internal class UserService
    {
        // private List<User> users = new();
        private IRepository<string, User> _repo;
        public UserService(IRepository<string,User> UserRepository)
        {
            _repo = UserRepository;
        }

        public void RegisterUser()
        {
            User user = GetUserDetails();
            user = _repo.Create(user);
            PrintUserDetails(user);
        }

        public User GetUserDetails()
        {
            var users = _repo.GetAllEntities() ?? new List<User>();
            
            string name;
            while (true)
            {
                Console.Write("Enter your name: ");
                name = Console.ReadLine() ?? "";

                if (!users.Any(u => u.Name == name))
                    break;

                Console.WriteLine($"{name} already exists!");
            }

            string email;
            while (true)
            {
                Console.Write("Enter your email address: ");
                email = Console.ReadLine() ?? "";
                if (email.Contains('@') && email.EndsWith(".com"))
                    break;
                Console.WriteLine("Invalid email format!");
            }
            if (users.Any(u => u.Email == email))
            {
                Console.WriteLine($"{email} is already linked with another account!");
                return null;
            }

            string phone;
            while (true)
            {
                Console.Write("Enter your 10-digit phone number: ");
                phone = Console.ReadLine() ?? "";
                if (phone.Length == 10 && phone.All(char.IsDigit))
                    break;
                Console.WriteLine("Invalid phone number!");
            }

            if (users.Any(u => u.Phone == phone))
            {
                Console.WriteLine($"{phone} already exists!");
                return null;
            }

            Console.Write("Is WhatsApp active on this number? (Y/N): ");
            string whatsappInput = Console.ReadLine()?.ToLower() ?? "n";
            bool hasWhatsapp = whatsappInput == "y" || whatsappInput == "yes";

            return new User(name, email, phone, hasWhatsapp);
        }
        public void PrintUserDetails(User user)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine(user);
            Console.WriteLine("---------------------------");
        }

        public void GetUserByName(string name)
        {
            var user = _repo.GetEntity(name);
            if(user == null)
            {
                Console.WriteLine($"User not found with username: {name}");
                return;
            }
            PrintUserDetails(user);
        }

        public void GetAllUsers()
        {
            if (!AdminCredentials())
            {
                Console.WriteLine("Access Denied!");
                return;
            }
            var users = _repo.GetAllEntities();
            if(users == null)
            {
                Console.WriteLine("No user set");
                return;
            }
            foreach(var user in users)
            {
                PrintUserDetails(user);
            }
        }

        bool AdminCredentials()
        {
            Console.WriteLine("Enter access key:");
            string accessKey = (Console.ReadLine()) ?? "";
            if(accessKey == "AdminRights")
            {
                return true;
            }
            return false;
        }

        public User? GetUserByEmail(string email)
        {
            var users = _repo.GetAllEntities();
            if(users == null)
            {
                Console.WriteLine("Empty user set");
                return null;
            }
            foreach (var user in users)
            {
                if(user.Email == email)
                {
                    PrintUserDetails(user);
                    return user;
                }
            }
            Console.WriteLine($"No user found with Email: {email}");
            return null;
        }

        public User? GetUserByPhone(string phone)
        {
            var users = _repo.GetAllEntities();
            if(users == null)
            {
                Console.WriteLine("Empty user set");
                return null;
            }
            foreach (var user in users)
            {
                if(user.Phone == phone)
                {
                    PrintUserDetails(user);
                    return user;
                }
            }
            Console.WriteLine($"No users found with Phone: {phone}");
            return null;
        }

        public void UpdateUser(string name)
        {
            var user = _repo.GetEntity(name);
            if(user == null)
            {
                Console.WriteLine($"User not found with name: {name}");
                return;
            }
            Console.WriteLine("Choose 1. Update Email\n 2.Update Phone\n Enter your choice:");
            int choice;
            while(!int.TryParse(Console.ReadLine(), out choice) && choice < 1 && choice  > 2)
            {
                Console.WriteLine("Invalid Entry");
            }
            Console.WriteLine("Enter new value:");
            string newValue = (Console.ReadLine() ?? "").Trim();
            if(choice == 1)
            {
                user.Email = newValue;

            }
            else
            {
                user.Phone = newValue;
            }
            var updatedUser = _repo.Update(name,user);
            PrintUserDetails(user);
            
        }

        public void DeleteUser(string name)
        {
            Console.Write($"Are you sure you want to delete '{name}'? (Y/N): ");
            string confirmation = (Console.ReadLine()?.ToLower()) ?? "n";
            if (confirmation != "y" && confirmation != "yes")
            {
                Console.WriteLine("  Deletion cancelled.");
                return;
            }
            var user = _repo.Delete(name);
            if( user == null)
            {
                Console.WriteLine("User doesn't exist");
            }
            Console.WriteLine($"  User '{user?.Name}' deleted successfully.");
        }

    }
}