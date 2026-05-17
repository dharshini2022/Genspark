namespace NotificationSystem.Exceptions
{
    public class InvalidMessageException : Exception
    {
        public InvalidMessageException(string input) : base(input)
        {
        }
    }
}
