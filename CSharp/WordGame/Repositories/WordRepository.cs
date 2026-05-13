using Npgsql;
using WordGame.Models;

namespace WordGame.Repositories
{
    internal class WordRepository
    {
        DbContext dBContext;

        public WordRepository()
        {
            dBContext = new DbContext();
        }

        public Word? GetRandomWord()
        {
            string selectQuery = "SELECT * FROM words ORDER BY RANDOM() LIMIT 1";
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand command = new NpgsqlCommand(selectQuery,connection);
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    return new Word(Convert.ToInt32(
                                        reader["id"]), 
                                        reader["word"].ToString()
                                    );
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return null;

            // Random random = new Random();
            // int index = random.Next(words.Count);
            // return words[index];
        }
    }
}