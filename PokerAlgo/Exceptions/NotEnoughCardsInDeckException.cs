namespace PokerAlgo;

[Serializable]
public class NotEnoughCardsInDeckException : PokerAlgoException
{
    public NotEnoughCardsInDeckException() { }
    public NotEnoughCardsInDeckException(string message) : base(message) { }
}