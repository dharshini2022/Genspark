using WordGame.Models;

namespace WordGame.Interfaces
{
    public interface IPlayerService
    {
        public string ValidateName(string name);
        public string ValidatePassword(string password);
        public Player? Register(string name, string password);
        public Player? Login(string name, string password);
        public Player? ViewProfile(int playerId);
        public void UpdateTotalScore(int playerId, int newGameScore);
        public List<Player> ViewLeaderBoard();
        public bool PlayerExists(string name);
        public bool ChangePassword(int playerId, string oldPassword, string newPassword);
    }
}