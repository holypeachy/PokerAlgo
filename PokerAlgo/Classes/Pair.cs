namespace PokerAlgo;
/// <summary>
/// Represents a pair of <see cref="Card"/>s — typically a player's hole cards in Texas Hold'em.
/// </summary>
public class Pair
{
    /// <summary>
    /// Gets the first card in the pair.
    /// </summary>
    public Card First { get; }
    /// <summary>
    /// Gets the second card in the pair.
    /// </summary>
    public Card Second { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pair"/> class using two cards.
    /// </summary>
    /// <param name="First">The first card.</param>
    /// <param name="Second">The second card.</param>
    public Pair(Card First, Card Second)
    {
        this.First = First;
        this.Second = Second;
    }

    public override string ToString()
    {
        return $"{First} {Second}";
    }
}