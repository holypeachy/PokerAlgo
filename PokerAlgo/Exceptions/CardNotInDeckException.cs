namespace PokerAlgo;
/// <summary>
/// The exception that is thrown when a specified card is not found in the deck.
/// Occurs when attempting to remove a card that doesn't exist in the deck, even if it has been dealt it should exist.
/// </summary>
[Serializable]
public class CardNotInDeckException : PokerAlgoException
{
    public CardNotInDeckException() { }
    public CardNotInDeckException(string message) : base(message) { }
}