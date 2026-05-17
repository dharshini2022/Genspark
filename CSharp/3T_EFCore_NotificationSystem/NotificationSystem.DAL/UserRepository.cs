using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using NotificationSystem.DAL.Interfaces;
using NotificationSystem.Models;
using NotificationSystem.DAL.Contexts;

namespace NotificationSystem.DAL
{
    public class UserRepository : IRepository<string, User>
    {
        private NotificationDbContext _dbContext;
        public UserRepository()
        {
            _dbContext = new NotificationDbContext();
        }

        public User? Create(User item)
        {
            try
            {
                _dbContext.users.Add(item);
                int rowsAffected = _dbContext.SaveChanges();
                if(rowsAffected == 0)
                {
                    Console.WriteLine("User Creation Failed");
                    return null;
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return item;
        }

        public User? GetEntity(string key)
        {
            try
            {
                return _dbContext.users.SingleOrDefault(u => u.Name == key);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public List<User>? GetAllEntities()
        {
            List<User> usersList = new List<User>();
            try
            {
                usersList = _dbContext.users.ToList();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return usersList.Count > 0 ? usersList : null;
        }

        public User? Update(string key, User user)
        {
            try
            {
                var exisitingUser = _dbContext.users.SingleOrDefault(u => u.Name == key);
                if(exisitingUser == null)   return null;

                exisitingUser.Email = user.Email ?? "";
                exisitingUser.Phone = user.Phone ?? "";
                exisitingUser.HasWhatsapp = user.HasWhatsapp;

                _dbContext.SaveChanges();
                return exisitingUser;

            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public User? Delete(string key)
        {
            try
            {
                var user = _dbContext.users.SingleOrDefault(u => u.Name == key);
                if(user == null)    return null;
                _dbContext.users.Remove(user);
                _dbContext.SaveChanges();
                return user;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
    }
}