namespace WordGame.Models
{
    public class Game
    {
        public int Id {get; set; }
        public int PlayerId { get; set; }
        public int WordId { get; set; }
        public string HiddenWord { get; set; } = string.Empty;
        public int MaxAttempts { get; set; } = 6;
        public int CurrentAttempt { get; set; } = 0;
        public bool IsWon { get; set; } = false;
        public int GameScore { get; set; } = 0;
        public List<string> PreviousGuesses { get; set; }

        public Game()
        {
            MaxAttempts = 6;
            CurrentAttempt = 0;
            GameScore = 0;
            PreviousGuesses = new List<string>();
        }
        public Game(string hiddenWord, int playerId, int wordId)
        {
            HiddenWord = hiddenWord;
            PlayerId = playerId;
            WordId = wordId;
            PreviousGuesses = new List<string>();
        }
    }
}