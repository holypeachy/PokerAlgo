namespace PokerAlgo;
/// <summary>
/// Represents the best five-card poker hand a player holds after evaluation.
/// </summary>
public class WinningHand
{
    /// <summary>
    /// Gets the type of the winning hand (e.g., Flush, Straight, Full House).
    /// </summary>
    public HandType Type { get; }
    /// <summary>
    /// Gets the list of cards that form the winning hand.
    /// </summary>
    public List<Card> Cards { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WinningHand"/> class with a hand type and the corresponding cards.
    /// </summary>
    /// <param name="type">The evaluated hand type.</param>
    /// <param name="cards">The list of cards that make up the winning hand.</param>
    public WinningHand(HandType type, List<Card> cards)
    {
        this.Type = type;
        this.Cards = cards;
    }

    public override string ToString()
    {
        return $"Type: {Type} " + $"| Cards: {string.Join(" ", Cards)}";
    }
}