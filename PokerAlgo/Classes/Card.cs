using System.Text.Json.Serialization;

namespace PokerAlgo;
public class Card : IEquatable<Card>
{
    public int Rank {get;}
    public CardSuit Suit {get;}
    public bool IsPlayerCard {get; set;}

    private static readonly Dictionary<int, string> _cardPrintLookUp = new()
    {
        {1, "A"}, {2, "2"}, {3, "3"}, {4, "4"}, {5, "5"}, {6, "6"}, {7, "7"}, {8, "8"}, {9, "9"}, {10, "T"},{11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}
    };


    [JsonConstructor]
    public Card(int rank, CardSuit suit, bool isPlayerCard)
    {
        if (rank < 1 || rank > 14) throw new InvalidCardRankException($"Rank value passed: {rank}. Values must be 1-14. Both 1 and 14 represent Ace.");

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
        return $"{_cardPrintLookUp[Rank]},{Suit}]" + (IsPlayerCard ? "🙂" : "");
    }

    // Does not take into consideration IsPlayerCard
    public bool Equals(Card? other)
    {
        if (other is null)
            return false;

        return Rank == other.Rank && Suit == other.Suit;
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