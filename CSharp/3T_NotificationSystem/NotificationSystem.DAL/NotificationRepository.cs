using Npgsql;
using NotificationSystem.Models;

namespace NotificationSystem.DAL
{
    public class NotificationRepository
    {
        private string _connectionString = "Host=localhost;Port=5432;Database=notificationsystem;Username=postgres;Password=12345";
        // List<Notification> notifications = new List<Notification>();
        private NpgsqlConnection _connection;

        public NotificationRepository()
        {
            _connection = new NpgsqlConnection(_connectionString);
        }

        public void SaveNotification(Notification notification)
        {
            string insertQuery = @"INSERT INTO notifications (Message, SentDate, SenderId, ReceiverId, Type, SenderContact, ReceiverContact) 
                                 VALUES (@Message, @SentDate, @SenderId, @ReceiverId, @Type, @SenderContact, @ReceiverContact)";
            try
            {
                _connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(insertQuery, _connection);
                command.Parameters.AddWithValue("@Message",notification.Message);
                command.Parameters.AddWithValue("@SentDate", notification.SentDate);
                command.Parameters.AddWithValue("@SenderID", notification.SenderId);
                command.Parameters.AddWithValue("@ReceiverID", notification.ReceiverId);
                command.Parameters.AddWithValue("@Type", (int) notification.Type);
                command.Parameters.AddWithValue("@SenderContact", notification.SenderContact);
                command.Parameters.AddWithValue("@ReceiverContact", notification.ReceiverContact);

                int rowsAffected = command.ExecuteNonQuery();
                if(rowsAffected == 0)
                {
                    Console.WriteLine("Notification Save Failed");
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _connection?.Close();
            }
        }

        public List<string>? GetNotificationByUsername(string name)
        {
            List<string> notifications = new List<string>();
            string joinQuery = @"SELECT n.Id, n.Message, n.SentDate, s.Name AS sender_name, r.Name AS receiver_name, n.Type, n.SenderContact, n.ReceiverContact
                                FROM notifications n JOIN users s ON n.SenderId = s.Id
                                JOIN users r ON n.ReceiverId = r.Id
                                WHERE s.Name = @name OR r.Name = @name
                                ORDER BY n.SentDate DESC";
            try
            {
                _connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(joinQuery, _connection);
                command.Parameters.AddWithValue("@name",name);
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string output = $"{reader["sentdate"]}\n From : {reader["sender_name"]}\n To   : {reader["receiver_name"]}\n Msg  : {reader["message"]}";
                    notifications.Add(output);
                }
                return notifications;

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
    }
}