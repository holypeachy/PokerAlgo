namespace PokerAlgo
{
    class Card
    {
        public int Value;
        public string Suit;
        public bool IsPlayerCard;

        public Card(int value, string suit, bool isPlayerCard = false)
        {
            this.Value = value;
            this.Suit = suit;
            this.IsPlayerCard = isPlayerCard;
        }

        public override string ToString()
        {
            // return $"[{Value},{Suit}]";
            return "[" + (Value == 1 ? "A" : Value <= 10 ? Value : Value == 11 ? "J" : Value == 12 ? "Q" : Value == 13 ? "K" : Value) + $",{Suit}]";
        }
    }
}