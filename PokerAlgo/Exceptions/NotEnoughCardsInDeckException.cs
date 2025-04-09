namespace PokerAlgo;
/// <summary>
/// The exception that is thrown when the deck does not contain enough cards to complete an operation.
/// </summary>
public class NotEnoughCardsInDeckException : PokerAlgoException
{
    public NotEnoughCardsInDeckException() { }
    public NotEnoughCardsInDeckException(string message) : base(message) { }
}