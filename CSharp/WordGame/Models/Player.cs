namespace WordGame.Models
{
    public partial class Player
    {
        public int Id {get; set; }
        public string Name {get; set; } = string.Empty;
        public string Password {get; set; } = string.Empty;
        public int Score {get; set; } = 0;

        public Player()
        {
            Score = 0;
        }
    }
}