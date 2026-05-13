using WordGame.Models;
using WordGame.Interfaces;
using WordGame.Repositories;
using WordGame.Exceptions;

namespace WordGame.Services
{
    internal class GameService : IGameService
    {
        private GameRepository _repo;
        public GameService()
        {
            _repo = new GameRepository();
        }

        public Game CreateGame(string hiddenWord, int playerId, int wordId)
        {
            return new Game(hiddenWord, playerId, wordId);
        }

        public bool CheckGuess(Game game, string guess)
        {
            game.CurrentAttempt++;

            if (guess == game.HiddenWord)
            {
                game.IsWon = true;
                Game newGame = _repo.SaveGame(game);
                return true;
            }
            return false;
        }

        public bool IsGameOver(Game game)
        {
            if(game.CurrentAttempt >= game.MaxAttempts || game.IsWon)
            {
                return true;
            }
            return false;
        }

        public void PrintColoredFeedback(string feedback)
        {
            foreach (char ch in feedback)
            {
                switch (ch)
                {
                    case 'G':
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;

                    case 'Y':
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case 'X':
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
                Console.Write(ch + " ");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        public List<Game> DisplayGamesByPlayerId(int player_id)
        {
            List<Game> games = _repo.GetGamesByPlayerId(player_id);
            if(games == null)
            {
                throw new EmptyResourceException("No Games Played!");
            }
            return games;
        }
    }
}