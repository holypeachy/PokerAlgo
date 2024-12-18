using System.Text.Json.Serialization;

namespace PokerAlgo
{
    class Card
    {
        public int Value {get; set;}
        public CardSuit Suit {get; set;}
        public bool IsPlayerCard {get; set;}

        private Dictionary<int, string> CardValueLookUp = new Dictionary<int, string>
        {
            {1, "A"}, {11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}
        };


        [JsonConstructor]
        public Card(int value, CardSuit suit, bool isPlayerCard)
        {
            this.Value = value;
            this.Suit = suit;
            this.IsPlayerCard = isPlayerCard;
        }

        public Card(Card other)
        {
            this.Value = other.Value;
            this.Suit = other.Suit;
            this.IsPlayerCard = other.IsPlayerCard;
        }


        public override string ToString()
        {
            return "[" + (Value == 1 || Value > 10 ? CardValueLookUp[Value] : Value) + $",{Suit}]" + (IsPlayerCard ? "🙂" : "");
        }

        public bool EqualsValue(Card other)
        {
            return this.Value == other.Value;
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

        public override int GetHashCode()   // Had to implement cus Equals override for some reason
        {
            return base.GetHashCode();
        }

    }
}