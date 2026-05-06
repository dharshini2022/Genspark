using WordGame.Models;

namespace WordGame.Interfaces
{
    internal interface IGame
    {
        public Game CreateGame(string hiddenWord);
        public bool CheckGuess(Game game, string guess);

        public bool IsGameOver(Game game);

        public void PrintColoredFeedback(string feedback);

    }
}