using NotificationSystem.Models;

public class ContactNotFoundException : Exception
{
    public ContactNotFoundException(string input) : base(input)
    {
        
    }
}