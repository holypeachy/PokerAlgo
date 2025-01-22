using System.Text.Json.Serialization;

namespace PokerAlgo
{
    public class Card
    {
        public int Rank {get; set;}
        public CardSuit Suit {get; set;}
        public bool IsPlayerCard {get; set;}

        private Dictionary<int, string> CardValueLookUp = new Dictionary<int, string>
        {
            {1, "A"}, {11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}
        };


        [JsonConstructor]
        public Card(int value, CardSuit suit, bool isPlayerCard)
        {
            this.Rank = value;
            this.Suit = suit;
            this.IsPlayerCard = isPlayerCard;
        }

        public Card(Card other)
        {
            this.Rank = other.Rank;
            this.Suit = other.Suit;
            this.IsPlayerCard = other.IsPlayerCard;
        }


        public override string ToString()
        {
            return "[" + (Rank == 1 || Rank > 10 ? CardValueLookUp[Rank] : Rank) + $",{Suit}]" + (IsPlayerCard ? "🙂" : "");
        }

        public override bool Equals(object? obj)
        {
            try
            {
                Card? other = (Card?)obj;

                return other is null ? false : ( this.Rank == other.Rank && this.Suit == other.Suit && this.IsPlayerCard == other.IsPlayerCard ); // ? Should I include IsPlayerCard?
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override int GetHashCode()   // Had to implement cus Equals override, for some reason
        {
            return base.GetHashCode();
        }

    }
}