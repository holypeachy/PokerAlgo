using System.Text.Json.Serialization;

namespace PokerAlgo
{
    class Card
    {
        public int Value {get; set;}
        public CardSuit Suit {get; set;}
        public bool IsPlayerCard {get; set;}

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

        // ! Need to have parametertless constructor for the JSON serializer. If I make copy ctor I have to declare it explicitly.
        // public Card(){}

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

        public bool EqualsNoSuit(Card other){
            return this.Value == other.Value && this.IsPlayerCard == other.IsPlayerCard;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}