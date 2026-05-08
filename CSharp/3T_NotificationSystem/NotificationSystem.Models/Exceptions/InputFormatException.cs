using NotificationSystem.Models;

public class InputFormatException : Exception
{
    public InputFormatException(string input) : base("Invalid input Format")
    {
        
    }
}