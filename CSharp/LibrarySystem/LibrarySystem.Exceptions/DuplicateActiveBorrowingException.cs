namespace LibrarySystem.Exceptions
{
    public class DuplicateActiveBorrowingException : Exception
    {
        public DuplicateActiveBorrowingException(string Message) : base(Message)
        {
            
        }
    }
}