namespace NotificationSystem.Exceptions
{
    public class ContactNotFoundException : Exception
    {
        public ContactNotFoundException(string input) : base(input)
        {
        }
    }
}
