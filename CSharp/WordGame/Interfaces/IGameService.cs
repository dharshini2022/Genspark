using WordGame.Models;

namespace WordGame.Interfaces
{
    internal interface IGameService
    {
        public Game? CreateGame(string hiddenWord, int playerId, int wordId);
        public bool CheckGuess(Game game, string guess);

        public bool IsGameOver(Game game);

        public void PrintColoredFeedback(string feedback);

    }
}