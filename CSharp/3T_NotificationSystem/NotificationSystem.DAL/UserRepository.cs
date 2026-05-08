using System.Security.Principal;
using NotificationSystem.DAL.Interfaces;
using NotificationSystem.Models;

namespace NotificationSystem.DAL
{
    public class UserRepository : IRepository<string, User>
    {
        Dictionary<string, User> userAccount = new Dictionary<string, User>();

        public User this[string index]
        {
            get{ return userAccount[index]; }
            set{ userAccount[index] = value; }
        }


        public User Create(User item)
        {
            userAccount.Add(item.Name, item);
            return item;
        }

        public  User? GetEntity(string key)
        {
            if (userAccount.ContainsKey(key))
            {
                return userAccount[key];
            }
            return null;
        }

        public List<User>? GetAllEntities()
        {
            if(userAccount.Count == 0)
            {
                return null;
            }
            var list = userAccount.Values.ToList();
            return list;
        }

        public User? Update(string key, User user)
        {
            if(user == null)
            {
                return null;
            }
            userAccount[key] = user;
            return user;
        }

        public User? Delete(string key)
        {
            var user = GetEntity(key);
            if(user == null){
                return null;
            }
            userAccount.Remove(key);
            return user;
        }
    }
}