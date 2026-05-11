using System.Data;
using System.Security.Principal;
using NotificationSystem.DAL.Interfaces;
using NotificationSystem.Models;
using Npgsql;

namespace NotificationSystem.DAL
{
    public class UserRepository : IRepository<string, User>
    {
        // Dictionary<string, User> userAccount = new Dictionary<string, User>();
        private string _connectionString = "Host=localhost;Port=5432;Database=notificationsystem;Username=postgres;Password=12345";
        private NpgsqlConnection _connection;


        // public User this[string index]
        // {
        //     get{ return userAccount[index]; }
        //     set{ userAccount[index] = value; }
        // }

        public UserRepository()
        {
            _connection = new NpgsqlConnection(_connectionString);
        }

        public User Create(User item)
        {
            string insertCmd = @"INSERT INTO users (Name,Email,Phone,HasWhatsapp) VALUES (@Name, @Email, @Phone, @Whatsapp)";
            try
            {
                _connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(insertCmd, _connection);
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Email",(object?)item.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone",(object?)item.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Whatsapp",item.HasWhatsapp);
                int generatedId = Convert.ToInt32(command.ExecuteScalar());
                item.Id = generatedId;
                // int rowsAffected = command.ExecuteNonQuery();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _connection.Close();
            }
            
            return item;
        }

        public User? GetEntity(string key)
        {
            string filterQuery = @"SELECT * FROM users WHERE Name = @key";
            try
            {
                _connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(filterQuery,_connection);
                command.Parameters.AddWithValue("@key",key);
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    return new User(
                        reader["Name"].ToString()!,
                        reader["Email"].ToString()!,
                        reader["Phone"].ToString()!,
                        Convert.ToBoolean(reader["HasWhatsapp"])
                    );
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _connection?.Close();
            }
            return null;
        }

        public List<User>? GetAllEntities()
        {
            List<User> users = new List<User>();
            string selectQuery = "SELECT * FROM users";
            try
            {
                _connection.Open();
                NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(selectQuery,_connection);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet,"Users");
                DataTable table = dataSet.Tables["Users"]!;
                foreach(DataRow dr in table.Rows)
                {
                    users.Add(new User(
                        dr["Name"].ToString()!,
                        dr["Email"].ToString()!,
                        dr["Phone"].ToString()!,
                        Convert.ToBoolean(dr["HasWhatsapp"])
                    ));
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _connection?.Close();
            }
            return users.Count > 0 ? users : null;
        }

        public User? Update(string key, User user)
        {
            if(user == null)
            {
                return null;
            }
            string updateQuery = @"UPDATE users SET Name = @Name, Email = @Email, Phone = @Phone, HasWhatsapp = @Whatsapp WHERE Name = @key";
            try
            {
                _connection.Open();        
                NpgsqlCommand command = new NpgsqlCommand(updateQuery,_connection);    
                //command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", (object ?)user.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone", (object ?)user.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Whatsapp",user.HasWhatsapp);
                command.Parameters.AddWithValue("@key",key);

                int rowsAffected = command.ExecuteNonQuery();
                if(rowsAffected == 0)   return null;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _connection?.Close();
            }
            return user;
        }

        public User? Delete(string key)
        {
            var user = GetEntity(key);
            string deleteQuery = @"DELETE FROM users WHERE Name = @key";
            try
            {
                _connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(deleteQuery,_connection);
                command.Parameters.AddWithValue("@key",key);
                int rowsAffected = command.ExecuteNonQuery();
                if(rowsAffected == 0)   return null;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _connection?.Close();
            }
            return user;
        }
    }
}