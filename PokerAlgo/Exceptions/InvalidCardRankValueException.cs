namespace PokerAlgo;

[Serializable]
public class InvalidCardRankException : PokerAlgoException
{
    public InvalidCardRankException() { }
    public InvalidCardRankException(string message) : base(message) { }
}