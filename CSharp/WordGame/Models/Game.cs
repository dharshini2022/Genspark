namespace WordGame.Models
{
    internal class Game
    {
        public int Id {get; set; }
        public int PlayerId { get; set; }
        public int WordId { get; set; }
        public string HiddenWord { get; set; } = string.Empty;
        public int MaxAttempts { get; set; } = 6;
        public int CurrentAttempt { get; set; } = 0;
        public bool IsWon { get; set; } = false;
        public int GameScore { get; set; } = 0;
        public HashSet<string> PreviousGuesses { get; set; }

        public Game()
        {
            
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