namespace WordGame.Models
{
    public class Word
    {
        public int Id {get; set; }
        public string WordName {get; set;} = string.Empty;

        public Word(int Id, string WordName)
        {
            this.Id = Id;
            this.WordName = WordName;
        }
    }
}