namespace PokerAlgo;
/// <summary>
/// The exception that is thrown when an attempt is made to draw a card from an empty deck.
/// </summary>
public class DeckEmptyException : PokerAlgoException
{
    public DeckEmptyException() { }
    public DeckEmptyException(string message) : base(message) { }
}