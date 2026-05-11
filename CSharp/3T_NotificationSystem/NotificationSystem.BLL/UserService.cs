using NotificationSystem.Models;
using NotificationSystem.DAL.Interfaces;
using NotificationSystem.Exceptions;
using DotNetEnv;

namespace NotificationSystem.BLL
{
    public class UserService
    {
        // private List<User> users = new();
        private IRepository<string, User> _repo;
        public UserService(IRepository<string,User> UserRepository)
        {
            _repo = UserRepository;
        }

        public void RegisterUser()
        {
            User? user = GetUserDetails();
            if(user == null)
            {
                return;
            }
            user = _repo.Create(user);
            PrintUserDetails(user);
        }

        public User? GetUserDetails()
        {
            var users = _repo.GetAllEntities() ?? new List<User>();
            string name;
            while (true)
            {
                Console.Write("Enter your name: ");
                name = (Console.ReadLine() ?? "" ).ToLower();

                if (!users.Any(u => u.Name == name))
                    break;

                throw new ExistingContactException($"User already exisits with username: {name}");
            }

            string email;
            while (true)
            {
                Console.Write("Enter your email address: ");
                email = Console.ReadLine() ?? "";
                if(!email.Contains('@') || !email.EndsWith(".com"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid Email ID!");
                    Console.ResetColor();
                    continue;
                }
                if(users.Any(u => u.Email == email))
                {
                    throw new ExistingContactException($"{email} already exists!");
                }
                break;
            }

            string phone;
            while (true)
            {
                Console.Write("Enter your 10-digit phone number: ");
                phone = Console.ReadLine() ?? "";
                if (phone.Length != 10 || !phone.All(char.IsDigit))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Enter a Valid Phone Number of Length 10");
                    Console.ResetColor();
                    continue;
                }
                if (users.Any(u => u.Phone == phone))
                {
                    throw new ExistingContactException($"{phone} already exisits");
                }
                break;
            }


            Console.Write("Is WhatsApp active on this number? (Y/N): ");
            string whatsappInput = Console.ReadLine()?.ToLower() ?? "n";
            bool HasWhatsapp = whatsappInput == "y" || whatsappInput == "yes";

            return new User(name, email, phone, HasWhatsapp);
        }
        public void PrintUserDetails(User user)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine(user);
            Console.WriteLine("---------------------------");
        }

        
        public User? GetUser(string name)
        {
            return _repo.GetEntity(name);
        }

        public void GetUserByName(string name)
        {
            var user = _repo.GetEntity(name);
            if(user == null)
            {
                throw new ContactNotFoundException($"User not found with username : {name}");
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
                throw new ContactNotFoundException("No Users Available");
            }
            foreach(var user in users)
            {
                PrintUserDetails(user);
            }
        }

        bool AdminCredentials()
        {
            Env.Load();
            string password = "AdminRights";
            Console.WriteLine(password);
            Console.WriteLine("Enter access key:");
            string accessKey = (Console.ReadLine()) ?? "";
            if(accessKey == password)
            {
                return true;
            }
            return false;
        }

        public User? GetUserByEmail(string email)
        {
            if(!email.Contains('@') || !email.EndsWith(".com"))
            {
                throw new InputFormatException("Invalid Email ID!");
            }
            var users = _repo.GetAllEntities();
            if(users == null)
            {
                throw new ContactNotFoundException("No Users Available");
            }
            foreach (var user in users)
            {
                if(user.Email == email)
                {
                    return user;
                }
            }
            throw new ContactNotFoundException($"User not found with email : {email}");
        }

        public User? GetUserByPhone(string phone)
        {
            if(phone.Trim().Length != 10 || !phone.All(char.IsDigit))
            {
                throw new InputFormatException("Enter a valid phone no. of length 10");
            }
            var users = _repo.GetAllEntities();
            if(users == null)
            {
                throw new ContactNotFoundException("No Users Available");
            }
            foreach (var user in users)
            {
                if(user.Phone == phone)
                {
                    return user;
                }
            }
            throw new ContactNotFoundException($"No users found with phone : {phone}");
        }

        public void UpdateUser(string name)
        {
            var user = _repo.GetEntity(name);
            if(user == null)
            {
                throw new ContactNotFoundException($"No users found with username : {name}");
            }
            Console.WriteLine("Choose 1. Update Email\n 2.Update Phone\n 3. Update Whatsapp Account Activitiy\n Enter your choice:");
            int choice;
            while(!int.TryParse(Console.ReadLine(), out choice) && choice < 1 && choice  > 3)
            {
                Console.WriteLine("Invalid Entry");
            }
            Console.WriteLine("Enter new value:");
            string newValue = (Console.ReadLine() ?? "").Trim();
            if(choice == 1)
            {
                user.Email = newValue;

            }
            else if(choice == 2)
            {
                user.Phone = newValue;
            }
            else
            {
                user.HasWhatsapp = !user.HasWhatsapp;
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
                throw new ContactNotFoundException($"User not found with username : {name}");
            }
            Console.WriteLine($"  User '{user?.Name}' deleted successfully.");
        }
    }
}