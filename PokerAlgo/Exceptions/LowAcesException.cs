namespace PokerAlgo;

[Serializable]
public class LowAcesException : PokerAlgoException
{
    public LowAcesException() { }
    public LowAcesException(string message) : base(message) { }
}