using WordGame.Models;
using WordGame.Repositories;
using WordGame.Exceptions;
using WordGame.Interfaces;

namespace WordGame.Services
{
    public class PlayerService : IPlayerService
    {
        private PlayerRepository _repo;
        public PlayerService()
        {
            _repo = new PlayerRepository();
        }

        public string ValidateName(string name)
        {
            if (PlayerExists(name))
            {
                throw new PlayerAlreadyExistsException($"Player already exists with username: {name}");
            }
            return name;

        }

        public string ValidatePassword(string password)
        {
            while (True)
            {
                if(password.Length < 5)
                {
                    throw new InvalidInputException("Password must contain minimum 5 characters");
                }
            }
        }
        public Player? Register(string name, string password)
        {
            Player player = new Player()
            {
                Name = name,
                Password = password,
                Score = 0
            };
            return _repo.CreatePlayer(player);
        }

        public Player? Login(string name, string password)
        {
            return _repo.GetPlayerByCredentials(name, password);
        }

        public Player? ViewProfile(int playerId)
        {
            return _repo.GetPlayerById(playerId);
        }

        public void UpdateTotalScore(int playerId, int newGameScore)
        {
            _repo.UpdateTotalScoreById(playerId, newGameScore);
        }

        public List<Player> ViewLeaderBoard()
        {
            List<Player> leaderBoard = new List<Player>();
            leaderBoard = _repo.GetLeaderBoard();
            if(leaderBoard == null)
            {
                Console.WriteLine("No List of Players");
            }
            return leaderBoard;
        }

        public bool PlayerExists(string name)
        {
            Player player = _repo.GetPlayerByName(name);
            return player != null;
        }

        public bool ChangePassword(int playerId, string oldPassword, string newPassword);
    }
}