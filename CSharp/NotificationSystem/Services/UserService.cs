using NotificationSystem.Models;

namespace NotificationSystem.Services
{
    internal class UserService
    {
        private List<User> users = new();

        public User RegisterUser()
        {
            User user = GetUserDetails();
            users.Add(user);
            return user;
        }

        public User GetUserDetails()
        {
            Console.Write("Enter your name: ");
            string name = Console.ReadLine()?.Trim() ?? "";

            string email = "";
            while (true)
            {
                Console.Write("Enter your email address: ");
                email = Console.ReadLine()?.Trim() ?? "";
                if (email.Contains('@') && email.EndsWith(".com"))
                    break;
                Console.WriteLine(" Invalid Entry! Email must contain '@' and end with '.com'.");
            }

            string phone = "";
            while (true)
            {
                Console.Write("Enter your 10-digit phone number: ");
                phone = Console.ReadLine()?.Trim() ?? "";
                if (phone.Length == 10 && phone.All(char.IsDigit))
                    break;
                Console.WriteLine(" Invalid Entry! Phone must be exactly 10 digits.");
            }

            Console.Write("Is WhatsApp active on this number? (Y/N): ");
            string whatsappInput = Console.ReadLine()?.Trim().ToLower() ?? "n";
            bool hasWhatsapp = whatsappInput == "y" || whatsappInput == "yes";

            return new User(name, email, phone, hasWhatsapp);
        }

        public void PrintUserDetails(User user)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine(user);
            Console.WriteLine("---------------------------");
        }

        public User? GetUserByEmail(string email)
        {
            foreach(User user in users){
                if(user.Email == email){
                    PrintUserDetails(user);
                    return user;
                }
            }
            Console.WriteLine($"No User registered in this email: {email}");
            return null;
        }

        public User? GetUserByPhone(string phone)
        {
            foreach(User user in users){
                if(user.Phone == phone){
                    return user;
                }
            }
            Console.WriteLine($"No User registered in this phone: {phone}");
            return null;
        }

        public void DeleteUser(User user)
        {
            Console.Write($"Are you sure you want to delete '{user.Name}'? (Y/N): ");
            string confirmation = Console.ReadLine()?.Trim().ToLower() ?? "n";
            if (confirmation != "y" && confirmation != "yes")
            {
                Console.WriteLine("  Deletion cancelled.");
                return;
            }
            users.Remove(user);
            Console.WriteLine($"  User '{user.Name}' deleted successfully.");
        }

    }
}