namespace WordGame.Models
{
    internal class LetterFeedback
    {
        public char correctLetterInCorrectPos {get; set; } = 'G';
        public char correctLetterInWrongPos {get; set; } = 'Y';
        public char wrongLetter {get; set; } = 'X';

        public LetterFeedback()
        {
            correctLetterInCorrectPos = 'G';
            correctLetterInWrongPos = 'Y';
            wrongLetter = 'X';
        }
    }
}