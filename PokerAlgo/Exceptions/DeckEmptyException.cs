namespace PokerAlgo;

[Serializable]
public class DeckEmptyException : PokerAlgoException
{
    public DeckEmptyException() { }
    public DeckEmptyException(string message) : base(message) { }
}