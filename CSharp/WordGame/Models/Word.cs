namespace WordGame.Models
{
    public class Word
    {
        public int Id {get; set; }
        public string Word {get; set;} = string.Empty;

        public Word(int Id, string Word)
        {
            this.Id = Id;
            this.Word = Word;
        }
    }
}