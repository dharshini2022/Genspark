namespace WordGame.Exceptions
{
    public class PlayerAlreadyExistsException : Exception
    {
        public PlayerAlreadyExistsException(string Message) : base(Message)
        {
            
        }
    }
}