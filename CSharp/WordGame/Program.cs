using WordGame.Exceptions;
using WordGame.Models;
using WordGame.Repositories;
using WordGame.Services;

namespace WordGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int score = 0;
            bool replay = true;

            while (replay)
            {
                WordRepository repository = new WordRepository();
                GameService gameService = new GameService();
                GuessValidator validator = new GuessValidator();
                FeedbackGenerator feedbackGenerator = new FeedbackGenerator();
                CommentGenerator commentGenerator = new CommentGenerator();

                string hiddenWord = repository.GetRandomWord();
                Game game = gameService.CreateGame(hiddenWord);

                Console.WriteLine("\nWORD GAME: ");
                while (!gameService.IsGameOver(game))
                {
                    Console.WriteLine($"\nAttempt No : {game.CurrentAttempt + 1} out of {game.TotalAttempts}");
                    Console.Write("Enter a Guess Word: ");
                    string guess = Console.ReadLine()!.ToUpper();
                    try
                    {
                        validator.Validate(guess);
                        if (game.PreviousGuesses.Contains(guess))
                        {
                            Console.WriteLine("Duplicate guess! Enter a different word");
                            continue;
                        }
                        game.PreviousGuesses.Add(guess);

                        string feedback = feedbackGenerator.GenerateFeedback(game.HiddenWord,guess);
                        gameService.PrintColoredFeedback(feedback);

                        bool result = gameService.CheckGuess(game, guess);
                        if (result)
                        {
                            score += 5;
                            string comment = commentGenerator.GetComment(game.CurrentAttempt);
                            Console.WriteLine($"{comment} You have won!");
                        }
                    }
                    catch (InvalidGuessException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                if (!game.IsCorrect)
                {
                    Console.WriteLine("\nGame Over!");
                    Console.WriteLine($"Hidden Word: {game.HiddenWord}");
                }

                Console.WriteLine($"\nCurrent Score : {score}");
                Console.Write("\nDo you want to replay? yes / no : ");

                string choice = Console.ReadLine()!.ToLower();

                if (choice != "yes")
                {
                    replay = false;
                }
            }

            Console.WriteLine("\nExisting the Game...");
        }
    }
}