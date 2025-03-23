using System.Text.Json.Serialization;

namespace PokerAlgo;
public class Card : IEquatable<Card>
{
    public int Rank {get;}
    public CardSuit Suit {get;}
    public bool IsPlayerCard {get; set;}

    private static readonly Dictionary<int, string> _cardPrintLookUp = new Dictionary<int, string>
    {
        {1, "A"}, {11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}
    };


    [JsonConstructor]
    public Card(int rank, CardSuit suit, bool isPlayerCard)
    {
        this.Rank = rank;
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
        return "[" + (Rank == 1 || Rank > 10 ? _cardPrintLookUp[Rank] : Rank) + $",{Suit}]" + (IsPlayerCard ? "🙂" : "");
    }

    public bool Equals(Card? other)
    {
        if (other is null)
            return false;

        return Rank == other.Rank && Suit == other.Suit; // ? Should I include IsPlayerCard?
    }

    public override bool Equals(object? obj)
    {
        if (obj is Card other)
            return Equals(other);
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Rank, Suit);
    }
    
}