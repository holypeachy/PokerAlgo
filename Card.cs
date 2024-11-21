namespace PokerAlgo
{
    class Card
    {
        public int Value;
        public string Suit;

        public Card(int value, string suit)
        {
            this.Value = value;
            this.Suit = suit;
        }

        public override string ToString()
        {
            return $"[{Value},{Suit}]";
        }
    }
}