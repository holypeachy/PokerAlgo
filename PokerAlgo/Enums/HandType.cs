/// <summary>
/// Represents the possible poker hand rankings used to evaluate a player's best five-card hand.
/// </summary>
public enum HandType
{
    /// <summary>
    /// No winning hand (high card only).
    /// </summary>
    Nothing,

    /// <summary>
    /// A single pair of cards with the same rank.
    /// </summary>
    Pair,

    /// <summary>
    /// Two separate pairs of cards.
    /// </summary>
    TwoPair,

    /// <summary>
    /// Three cards of the same rank.
    /// </summary>
    ThreeKind,

    /// <summary>
    /// Five cards in numerical sequence, regardless of suit.
    /// </summary>
    Straight,

    /// <summary>
    /// Five cards of the same suit, not in sequence.
    /// </summary>
    Flush,

    /// <summary>
    /// A pair and a three-of-a-kind.
    /// </summary>
    FullHouse,

    /// <summary>
    /// Four cards of the same rank.
    /// </summary>
    FourKind,

    /// <summary>
    /// Five cards in sequence, all of the same suit.
    /// </summary>
    StraightFlush,

    /// <summary>
    /// An Ace-high straight flush (A-K-Q-J-10 of the same suit).
    /// </summary>
    RoyalFlush
}