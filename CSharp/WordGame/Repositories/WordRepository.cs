namespace WordGame.Repositories
{
    internal class WordRepository
    {
        private List<string> _words;

        public WordRepository()
        {
            _words = new List<string>
            {
                "APPLE",
                "MANGO",
                "GRAPE",
                "TRAIN",
                "PLANT",
                "BRAIN"
            };
        }

        public string GetRandomWord()
        {
            Random random = new Random();

            int index = random.Next(_words.Count);

            return _words[index];
        }
    }
}