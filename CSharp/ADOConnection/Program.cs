using Npgsql;
using System.Net.NetworkInformation;

namespace UnderstandingADOApp
{
    
    internal class Program
    {
        string connectionString =
            "Host=localhost;Port=5432;Database=demo;Username=postgres;Password=12345";
        NpgsqlConnection connection;
        public Program()
        {
          connection = new NpgsqlConnection(connectionString);
           
        }
        void GetUserDataFromDatabase()
        {
            string selectQuery = "Select * from NewUsers";
            NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("UserName : " + reader[0].ToString());
                    Console.WriteLine("Password : " + reader[1].ToString());
                }
                Console.WriteLine("Done reading");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

        }

        void GetFilterData(string column, string value)
        {
            string selectQuery = $"Select * from NewUsers WHERE {column} = '{value}'";
            NpgsqlCommand command = new NpgsqlCommand(selectQuery,connection);
            try
            {
                // connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("Username: " + reader[0]);
                    Console.WriteLine("Password: " + reader[0]);
                    Console.WriteLine("Role: " + reader[0]);

                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // connection.Close();
            }

        }
        void UpdatePassword()
        {
            //User user = GetUserDataFromConsole();
            Console.Write("Enter username for updation:");
            string username = Console.ReadLine() ?? "";
            Console.Write("Enter new password:");
            string password = Console.ReadLine() ?? "";
            string updateCmd = $"UPDATE NewUsers SET PassWord = '{password}' WHERE Username = '{username}'";
            NpgsqlCommand command = new NpgsqlCommand(updateCmd,connection);
            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                if(result > 0)
                {
                    Console.WriteLine("Updation Successful");
                    GetFilterData("Username",username);
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }finally
            {
                connection.Close();
            }
        }

        void InsertUserInToDatabase()
        {
            User user = GetUserDataFromConsole();
            string insertCmd = $"Insert into NewUsers values('{user.Username}','{user.Password}','{user.Role}')";
            NpgsqlCommand command = new NpgsqlCommand(insertCmd, connection);
            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                if(result>0)
                    Console.WriteLine("User created successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                connection?.Close();
            }
        }

        private User GetUserDataFromConsole()
        {
            User user = new User();
            Console.WriteLine("Please eneter your preffered username");
            user.Username = Console.ReadLine()??"";
            Console.WriteLine("Please eneter teh password");
            user.Password = Console.ReadLine()??"";
            Console.WriteLine("Please eneter your role");
            user.Role = Console.ReadLine() ?? "";
            return user;

        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Program program = new Program();

            program.InsertUserInToDatabase();
            program.UpdatePassword();
            program.GetUserDataFromDatabase();
        }
    }
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
