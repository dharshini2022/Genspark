namespace NotificationSystem.Exceptions
{
    public class ExistingContactException : Exception
    {
        public ExistingContactException(string input) : base(input)
        {
        }
    }
}
