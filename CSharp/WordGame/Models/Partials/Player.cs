namespace WordGame.Models
{
    public partial class Player
    {
        public override string ToString()
        {
            return $"Name: {Name}\n Password: {Password}\n Total Score: {Score}";
        }
    }
}