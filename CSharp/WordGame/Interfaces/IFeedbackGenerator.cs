namespace WordGame.Interfaces
{
    internal interface IFeedbackGenerator
    {
        string GenerateFeedback(string hiddenWord, string guess);
    }
}