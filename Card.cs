namespace PokerAlgo
{
    class Card
    {
        public int Value;
        public CardSuit Suit;
        public bool IsPlayerCard;

        public Card(int value, CardSuit suit, bool isPlayerCard)
        {
            this.Value = value;
            this.Suit = suit;
            this.IsPlayerCard = isPlayerCard;
        }

        public override string ToString()
        {
            // return $"[{Value},{Suit}]";
            return "[" + (Value == 1 || Value == 14 ? "A" : Value <= 10 ? Value : Value == 11 ? "J" : Value == 12 ? "Q" : Value == 13 ? "K" : Value) + $",{Suit}]";
        }
    }
}