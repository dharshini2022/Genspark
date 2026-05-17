namespace WordGame.Models
{
    public partial class Player
    {
        public override string ToString()
        {
            return $"Name: {Name}\nPassword: {Password}\nTotal Score: {Score}";
        }
    }
}