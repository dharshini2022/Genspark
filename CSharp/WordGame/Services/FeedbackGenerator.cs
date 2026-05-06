using System.Text;
using System.Linq;
using WordGame.Interfaces;
using WordGame.Models;


namespace WordGame.Services
{
    internal class FeedbackGenerator : IFeedbackGenerator
    {
        public string GenerateFeedback(string hiddenWord, string guessWord)
        {
            StringBuilder Feedback = new StringBuilder();
            for(int idx = 0; idx < guessWord.Length; ++idx)
            {
                int count = guessWord.Count(ch => ch == Convert.ToChar(hiddenWord[idx]));
                if(hiddenWord[idx] == guessWord[idx])
                {
                    Feedback.Append('G');
                    count--;
                }else if (hiddenWord.Contains(guessWord[idx]))
                {
                    Feedback.Append('Y');
                    count--;
                }
                else
                {
                    Feedback.Append('X');
                }
            }

            return Feedback.ToString();
        }
    }
}