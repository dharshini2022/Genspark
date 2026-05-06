namespace WordGame.Exceptions
{
    internal class InvalidGuessException : Exception
    {
        public InvalidGuessException(string message)
            : base(message)
        {
        }
    }
}