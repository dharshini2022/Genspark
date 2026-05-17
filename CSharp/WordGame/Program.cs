using System.Drawing;
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
            try
            {
                while (running)
                {
                    SetConsoleForeGroundColor("\n===== WORD GAME =====");
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
                            SetConsoleForeGroundColor("Exiting Game...");
                            break;

                        default:
                            SetConsoleForeGroundColor("Invalid Choice! Enter choice between 1 to 3","Red");
                            break;
                    }
                }
                
            }catch(EmptyResourceException ex)
            {
                SetConsoleForeGroundColor($"{ex.Message}","Red");
            }catch(InvalidGuessException ex)
            {
                SetConsoleForeGroundColor(ex.Message,"Red");
            }catch(InvalidInputException ex)
            {
                SetConsoleForeGroundColor(ex.Message,"Red");
            }catch(PlayerAlreadyExistsException ex)
            {
                SetConsoleForeGroundColor(ex.Message,"Red");
            }catch(Exception ex)
            {
                SetConsoleForeGroundColor(ex.Message,"Red");
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
                    SetConsoleForeGroundColor("Registration Successful!");
                }
            }
            catch (Exception ex)
            {
                SetConsoleForeGroundColor(ex.Message,"Red");
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
                SetConsoleForeGroundColor("Invalid Credentials","Red");
                return null;
            }

            SetConsoleForeGroundColor($"Welcome {player.Name}");
            return player;
        }

        static void LoggedInMenu( Player player, PlayerService playerService, GameService gameService, WordRepository wordRepository)
        {
            bool loggedIn = true;
            while (loggedIn)
            {
                SetConsoleForeGroundColor("\n===== PLAYER MENU =====");
                Console.WriteLine("1. Play New Game");
                Console.WriteLine("2. View Profile");
                Console.WriteLine("3. View Leaderboard");
                Console.WriteLine("4. View Game History");
                Console.WriteLine("5. Logout");

                Console.Write("Enter Choice: ");
                int choice = Convert.ToInt32(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        StartGame(player, playerService, gameService, wordRepository);
                        break;

                    case 2:
                        SetConsoleForeGroundColor($"Profile of {player.Name}:");
                        ViewProfile(player, playerService);
                        break;

                    case 3:
                        SetConsoleForeGroundColor("LeaderBoard:");
                        ViewLeaderBoard(playerService);
                        break;

                    case 4:
                        SetConsoleForeGroundColor($"Game History of {player.Name}:");
                        ViewGameHistory(player, gameService);
                        break;
                    case 5:
                        SetConsoleForeGroundColor("Logging off");
                        loggedIn = false;
                        break;

                    default:
                        SetConsoleForeGroundColor("Invalid Choice! Enter choice between 1 to 5!","Red");
                        break;
                }
            }
        }

        static void StartGame( Player player, PlayerService playerService, GameService gameService, WordRepository wordRepository)
        {
            GuessValidator validator = new GuessValidator();
            FeedbackGenerator feedbackGenerator = new FeedbackGenerator();
            CommentGenerator commentGenerator = new CommentGenerator();

            Word hiddenWord = wordRepository.GetRandomWord();
            Game game = gameService.CreateGame(hiddenWord.WordName,player.Id,hiddenWord.Id);

            SetConsoleForeGroundColor("\n===== GAME STARTED =====");
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
                        SetConsoleForeGroundColor("Duplicate Guess!","Red");
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
                        SetConsoleForeGroundColor("You Won!");
                        break;
                    }
                }
                catch (InvalidGuessException ex)
                {
                    SetConsoleForeGroundColor(ex.Message,"Red");
                }
            }

            if (!game.IsWon)
            {
                SetConsoleForeGroundColor("Game Over!","Red");
                Console.WriteLine($"Correct Word: {game.HiddenWord}");
            }
            Game savedGame = gameService.SaveGame(game);
        }

        static void ViewProfile(Player currentPlayer, PlayerService playerService)
        {
            Player? player = playerService.ViewProfile(currentPlayer.Id);
            if (player != null)
            {
                SetConsoleForeGroundColor("\n===== PROFILE =====");
                // Console.WriteLine($"Name: {player.Name}");
                // Console.WriteLine($"Password: {player.Password}");
                // Console.WriteLine($"Score: {player.Score}");
                Console.WriteLine(player);
            }
        }

        static void ViewLeaderBoard(PlayerService playerService)
        {
            List<Player> players = playerService.ViewLeaderBoard();

            SetConsoleForeGroundColor("\n===== LEADERBOARD =====");
            Console.WriteLine("Name - Score");
            foreach (Player player in players)
            {
                Console.WriteLine( $"{player.Name} - {player.Score}");
            }
        }

        static void ViewGameHistory(Player player, GameService gameService)
        {
            List<Game> gameHistory = gameService.DisplayGamesByPlayerId(player.Id);
            foreach(Game game in gameHistory)
            {
                Console.WriteLine("========================");
                Console.WriteLine(game);
                Console.WriteLine("========================");
            }
        }

        static void SetConsoleForeGroundColor(string text, string color = "Green")
        {
            Console.ForegroundColor = color == "Green" ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}