namespace WordGame.Models
{
    internal class Game
    {
        public string HiddenWord {get; set; }
        public int TotalAttempts = 6;
        public int CurrentAttempt {get; set; } = 0;
        public bool IsCorrect {get; set; } = false;

        public List<string> PreviousGuesses;

        public Game(string HiddenWord)
        {
            this.HiddenWord = HiddenWord;
            CurrentAttempt = 0;
            IsCorrect = false;
            PreviousGuesses = new List<string>();
        }
    }
}