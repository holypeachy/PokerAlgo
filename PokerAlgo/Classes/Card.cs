using System.Text.Json.Serialization;

namespace PokerAlgo;
/// <summary>
/// Represents a single playing card in a standard 52-card French deck.
/// </summary>
public class Card : IEquatable<Card>
{
    /// <summary>
    /// Gets the rank of the card (1–14), where both 1 and 14 represent an Ace.
    /// </summary>
    public int Rank { get; }
    /// <summary>
    /// Gets the suit of the card: Spades, Clubs, Hearts, or Diamonds.
    /// </summary>
    public CardSuit Suit { get; }
    /// <summary>
    /// Gets or sets whether this card belongs to a player. Used for rendering or debugging.
    /// </summary>
    public bool IsPlayerCard { get; set; }

    private static readonly Dictionary<int, string> _cardPrintLookUp = new()
    {
        {1, "A"}, {2, "2"}, {3, "3"}, {4, "4"}, {5, "5"}, {6, "6"}, {7, "7"}, {8, "8"}, {9, "9"}, {10, "T"},{11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Card"/> class with the given rank, suit, and player ownership.
    /// </summary>
    /// <param name="rank">An integer from 1 to 14, where 1 and 14 represent an Ace. When manually instantiating Aces use rank 14.</param>
    /// <param name="suit">The suit of the card.</param>
    /// <param name="isPlayerCard">Whether this card belongs to a player.</param>
    /// <exception cref="InvalidCardRankException"></exception>
    [JsonConstructor]
    public Card(int rank, CardSuit suit, bool isPlayerCard)
    {
        if (rank < 1 || rank > 14) throw new InvalidCardRankException($"Rank value passed: {rank}. Values must be 1-14. Both 1 and 14 represent Ace.");

        this.Rank = rank;
        this.Suit = suit;
        this.IsPlayerCard = isPlayerCard;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Card"/> class by deep-copying another <see cref="Card"/>.
    /// </summary>
    /// <param name="other">The card to copy.</param>
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