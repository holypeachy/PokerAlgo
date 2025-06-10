namespace PokerAlgo;

[Serializable]
public class CardNotInDeckException : PokerAlgoException
{
    public CardNotInDeckException() { }
    public CardNotInDeckException(string message) : base(message) { }
}