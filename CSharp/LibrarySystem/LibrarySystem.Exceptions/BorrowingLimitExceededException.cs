namespace LibrarySystem.Exceptions
{
    public class BorrowingLimitExceededException : Exception
    {
        public BorrowingLimitExceededException(string Message) : base(Message)
        {
            
        }
    }
}