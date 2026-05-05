using NotificationSystem.Models;

namespace NotificationSystem.Interfaces
{
    internal interface IUser
    {
        void RegisterUser();
        User GetUserDetails();
        void PrintUserDetails(User user);
        void GetUserByName(string name);
        void GetUserByEmail(string email);
        void GetUserByPhone(string phone);

        void GetAllUsers();
        bool AdminCredentials();
        void UpdateUser(string name);
        void DeleteUser(string name);
    }
}