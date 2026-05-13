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
            PlayerService playerService = new PlayerService();
            GameService gameService = new GameService();
            WordRepository wordRepository = new WordRepository();

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n===== WORD GAME =====");
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Exit");

                Console.Write("Enter Choice: ");
                int choice = Convert.ToInt32(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        Register(playerService);
                        break;

                    case 2:
                        Player? loggedInPlayer = Login(playerService);
                        if (loggedInPlayer != null)
                        {
                            LoggedInMenu( loggedInPlayer, playerService, gameService, wordRepository);
                        }
                        break;

                    case 3:
                        running = false;
                        Console.WriteLine("Exiting Game...");
                        break;

                    default:
                        Console.WriteLine("Invalid Choice");
                        break;
                }
            }
        }

        static void Register(PlayerService playerService)
        {
            try
            {
                Console.Write("\nEnter Username: ");
                string name = Console.ReadLine()!;
                playerService.ValidateName(name);

                Console.Write("Enter Password: ");
                string password = Console.ReadLine()!;
                playerService.ValidatePassword(password);

                Player? player = playerService.Register(name, password);
                if (player != null)
                {
                    Console.WriteLine("Registration Successful!");
                    // Login(playerService);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static Player? Login(PlayerService playerService)
        {
            Console.Write("\nEnter Username: ");
            string name = Console.ReadLine()!;
            Console.Write("Enter Password: ");
            string password = Console.ReadLine()!;
            Player? player = playerService.Login(name, password);

            if (player == null)
            {
                Console.WriteLine("Invalid Credentials");
                return null;
            }

            Console.WriteLine($"Welcome {player.Name}");
            return player;
        }

        static void LoggedInMenu( Player player, PlayerService playerService, GameService gameService, WordRepository wordRepository)
        {
            bool loggedIn = true;
            while (loggedIn)
            {
                Console.WriteLine("\n===== PLAYER MENU =====");
                Console.WriteLine("1. Play New Game");
                Console.WriteLine("2. View Profile");
                Console.WriteLine("3. View Leaderboard");
                Console.WriteLine("4. Logout");

                Console.Write("Enter Choice: ");
                int choice = Convert.ToInt32(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        StartGame(player, playerService, gameService, wordRepository);
                        break;

                    case 2:
                        ViewProfile(player, playerService);
                        break;

                    case 3:
                        ViewLeaderBoard(playerService);
                        break;

                    case 4:
                        loggedIn = false;
                        Console.WriteLine("Logged Out Successfully");
                        break;

                    default:
                        Console.WriteLine("Invalid Choice");
                        break;
                }
            }
        }

        static void StartGame( Player player, PlayerService playerService, GameService gameService, WordRepository wordRepository)
        {
            GuessValidator validator = new GuessValidator();
            FeedbackGenerator feedbackGenerator = new FeedbackGenerator();
            CommentGenerator commentGenerator = new CommentGenerator();

            string hiddenWord = wordRepository.GetRandomWord();
            Game game = gameService.CreateGame(hiddenWord,player.Id,1);

            Console.WriteLine("\n===== GAME STARTED =====");
            while (!gameService.IsGameOver(game))
            {
                Console.WriteLine($"\nAttempt {game.CurrentAttempt + 1} out of {game.MaxAttempts}");
                Console.Write("Enter Guess: ");
                string guess = Console.ReadLine()!.ToLower();
                try
                {
                    validator.Validate(guess);
                    if (game.PreviousGuesses.Contains(guess))
                    {
                        Console.WriteLine("Duplicate Guess!");
                        continue;
                    }

                    game.PreviousGuesses.Add(guess);
                    string feedback = feedbackGenerator.GenerateFeedback( game.HiddenWord, guess);
                    gameService.PrintColoredFeedback(feedback);
                    bool result = gameService.CheckGuess(game, guess);

                    if (result)
                    {
                        int score = 5;
                        game.GameScore = score;
                        playerService.UpdateTotalScore( player.Id, score );
                        Console.WriteLine(commentGenerator.GetComment(game.CurrentAttempt));
                        Console.WriteLine("You Won!");
                        break;
                    }
                }
                catch (InvalidGuessException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (!game.IsWon)
            {
                Console.WriteLine("\nGame Over!");
                Console.WriteLine(
                    $"Correct Word: {game.HiddenWord}");
            }
        }

        static void ViewProfile(Player currentPlayer, PlayerService playerService)
        {
            Player? player = playerService.ViewProfile(currentPlayer.Id);
            if (player != null)
            {
                Console.WriteLine("\n===== PROFILE =====");
                Console.WriteLine($"Name: {player.Name}");
                Console.WriteLine($"Password: {player.Password}");
                Console.WriteLine($"Score: {player.Score}");
                // Console.WriteLine(player);
            }
        }

        static void ViewLeaderBoard(PlayerService playerService)
        {
            List<Player> players = playerService.ViewLeaderBoard();

            Console.WriteLine("\n===== LEADERBOARD =====");
            Console.WriteLine("Name - Score");
            foreach (Player player in players)
            {
                Console.WriteLine( $"{player.Name} - {player.Score}");
            }
        }
    }
}