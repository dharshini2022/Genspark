using NotificationSystem.Models;

public class InvalidMessageException : Exception
{
    public InvalidMessageException(string message) : base(message)
    {
    }
}