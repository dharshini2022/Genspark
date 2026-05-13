using System.Data.Common;
using System.Reflection.Metadata;
using System.Security;
using WordGame.Models;
using Npgsql;

namespace WordGame.Repositories
{
    public class PlayerRepository
    {
        DbContext dBContext;
        public PlayerRepository()
        {
            dBContext = new DbContext();
        }

        public Player? CreatePlayer(Player player)
        {
            string insertQuery = @"INSERT INTO players (name, password) VALUES (@name, @password) RETURNING id";
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand command = new NpgsqlCommand(insertQuery,connection);
                command.Parameters.AddWithValue("@name", player.Name);
                command.Parameters.AddWithValue("@password", player.Password);
                int generatedPlayerId = Convert.ToInt32(command.ExecuteScalar());
                player.Id = generatedPlayerId;
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return player ?? null;   
        }

        public void UpdateTotalScoreById(int player_id, int newGameScore)
        {
            string updateQuery = @"UPDATE players SET score = score + @newGameScore WHERE id = @player_id";
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand command = new NpgsqlCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@newGameScore", newGameScore);
                command.Parameters.AddWithValue("@player_id", player_id);
                command.ExecuteNonQuery();
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
        }

        public List<Player> GetLeaderBoard()
        {
            List<Player> leaderBoard = new List<Player>();
            string leaderBoardQuery = "SELECT name, score FROM players ORDER BY score DESC";
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand command = new NpgsqlCommand(leaderBoardQuery, connection);
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    leaderBoard.Add( new Player()
                    {
                        Name = reader["name"].ToString(),
                        Score = Convert.ToInt32(reader["score"])
                    });
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
            return leaderBoard.Count > 0 ? leaderBoard : null;
        }

        public Player? GetPlayerById(int player_id)
        {
            string selectQuery = @"SELECT * FROM players WHERE Id = @player_id";
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection);
                command.Parameters.AddWithValue("@player_id", player_id);
                using NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new Player()
                    {
                        Name = reader["name"].ToString(),
                        Password = reader["password"].ToString(),
                        Score = Convert.ToInt32(reader["score"])
                    };
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return null;
        }

        public Player? GetPlayerByCredentials(string name, string password)
        {
            string filterQuery = @"SELECT * FROM players WHERE name = @name AND password = @password";
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand command = new NpgsqlCommand(filterQuery, connection);
                command.Parameters.AddWithValue("@name",name);
                command.Parameters.AddWithValue("@password",password);
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    return new Player()
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString(),
                        Password = reader["password"].ToString(),
                        Score = Convert.ToInt32(reader["score"])
                    };
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return null;
        }

        public Player? GetPlayerByName(string name)
        {
            string selectQuery = @"SELECT * FROM players WHERE name = @name"; 
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection);
                command.Parameters.AddWithValue("@name",name);
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    return new Player()
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString(),
                        Password = reader["password"].ToString(),
                        Score = Convert.ToInt32(reader["score"])
                    };
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection?.Close();
            }  
            return null;
        }
    }
}