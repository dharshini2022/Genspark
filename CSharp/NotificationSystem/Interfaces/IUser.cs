using NotificationSystem.Models;

namespace NotificationSystem.Interfaces
{
    internal interface IUser
    {
        void RegisterUser();
        User GetUserDetails();
        void PrintUserDetails(User user);
        void GetUserByEmail(string email);
        void GetUserByPhone(string phone);
        void DeleteUser(User user);
    }
}