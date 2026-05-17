namespace WordGame.Models
{
    public partial class Game
    {
        public override string ToString()
        {
            return $"Word: {HiddenWord}\nScore: {GameScore}\nResult: {IsWon}\nGameDateTime: {GameDateTime}";
        }
    }
}