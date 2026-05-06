namespace WordGame.Services
{
    internal class CommentGenerator
    {
        public string GetComment(int attempts)
        {
            switch (attempts)
            {
                case 1:
                    return "Genius!";

                case 2:
                    return "Excellent!";

                case 3:
                    return "Great job!";

                case 4:
                    return "Good work!";

                case 5:
                    return "Nice try!";

                case 6:
                    return "That was close!";

                default:
                    return "";
            }
        }
    }
}