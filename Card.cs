namespace PokerAlgo
{
    class Card
    {
        public int Value {get; set;}
        public CardSuit Suit {get; set;}
        public bool IsPlayerCard {get; set;}

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

        public override bool Equals(object? obj)
        {
            try
            {
                Card? other = (Card?)obj;

                return other is null ? false : (this.Value == other.Value && this.Suit == other.Suit && this.IsPlayerCard == other.IsPlayerCard);
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}