namespace WordGame.Repositories
{
    internal class WordRepository
    {
        DBContext dBContext;

        public WordRepository()
        {
            dBContext = new DBContext();
        }

        public string GetRandomWord()
        {
            List<string> words = new List<string>();
            string selectQuery = "SELECT * FROM words";
            using NpgsqlConnection connection = dBContext.GetConnection();
            try
            {
                connection.Open();
                using NpgsqlCommand command = new NpgsqlCommand(selectQuery,connection);
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    words.Add(reader[1].ToString);
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection.Close();
            }

            Random random = new Random();
            int index = random.Next(words.Count);
            return words[index];
        }
    }
}