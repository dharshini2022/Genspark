using WordGame.Models;
using WordGame.Interfaces;

namespace WordGame.Services
{
    internal class GameService : IGame
    {
        public Game CreateGame(string hiddenWord)
        {
            return new Game(hiddenWord);
        }

        public bool CheckGuess(Game game, string guess)
        {
            game.CurrentAttempt++;

            if (guess == game.HiddenWord)
            {
                game.IsCorrect = true;

                return true;
            }

            return false;
        }

        public bool IsGameOver(Game game)
        {
            if(game.CurrentAttempt >= game.TotalAttempts || game.IsCorrect)
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
    }
}