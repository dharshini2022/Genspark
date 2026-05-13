using System.Data;
using WordGame.Models;
using Npgsql;

namespace WordGame.Repositories
{
    public class GameRepository
    {
        DbContext dBContext;
        public GameRepository()
        {
            dBContext = new DbContext();
        }

        public Game? SaveGame(Game game)
        {
            string insertQuery = @"INSERT INTO games (player_id, word_id, game_score, is_won)
                VALUES (@playerId, @wordId, @gameScore, @isWon)
                RETURNING id;";
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@playerId", game.PlayerId);
                insertCommand.Parameters.AddWithValue("@wordId", game.WordId);
                insertCommand.Parameters.AddWithValue("@gameScore", game.GameScore);
                insertCommand.Parameters.AddWithValue("@isWon", game.IsWon);
                int generatedGameId = Convert.ToInt32(insertCommand.ExecuteScalar());
                game.Id = generatedGameId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            return game ?? null;
        }

        public List<Game>? GetGamesByPlayerId(int player_id)
        {
            List<Game> games = new List<Game>();
            string selectQuery = @"SELECT * FROM games WHERE player_id = @player_id";
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection);
                command.Parameters.AddWithValue("@player_id",player_id);
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    games.Add( new Game()
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        IsWon = Convert.ToBoolean(reader["is_won"]),
                        GameScore = Convert.ToInt32(reader["game_score"])
                    });
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
            return games.Count > 0 ? games : null;
        }

    }
}