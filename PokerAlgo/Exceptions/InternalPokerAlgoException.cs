namespace PokerAlgo;

[Serializable]
public class InternalPokerAlgoException : PokerAlgoException
{
    public InternalPokerAlgoException() { }
    public InternalPokerAlgoException(string message) : base(message) { }
}