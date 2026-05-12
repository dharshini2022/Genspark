using Npgsql;
namespace WordGame.Repositories
{
    public class DbContext
    {
        private string _connectionString = "Host=localhost;Port=5432;Database=WordGame;Username=postgres;Password=12345";

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
        

    }
}