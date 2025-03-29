namespace PokerAlgo;

[Serializable]
public class PokerAlgoException : Exception
{
    public PokerAlgoException() { }
    public PokerAlgoException(string message) : base(message) { }
}