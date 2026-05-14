namespace NotificationSystem.Exceptions
{
    public class InputFormatException : Exception
    {
        public InputFormatException(string input) : base(input)
        {
        }
    }
}
