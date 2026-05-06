using WordGame.Exceptions;
using WordGame.Interfaces;

namespace WordGame.Services
{
    internal class GuessValidator : IGuessValidator
    {
        public bool Validate(string guess)
        {
            if (string.IsNullOrEmpty(guess))
            {
                throw new InvalidGuessException("Empty Input Entered!");
            }
            if(guess.Length != 5)
            {
                throw new InvalidGuessException("Input should contain 5 letters");
            }
            if (guess.Any(char.IsDigit))
            {
                throw new InvalidGuessException("Input should not contain any digits");
            }
            if(guess.Any(ch => !char.IsLetter(ch)))
            {
                throw new InvalidGuessException("Input should not contain any special characters");
            }
            return true;
        }
    }
}