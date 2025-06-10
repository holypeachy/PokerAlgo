namespace PokerAlgo;

[Serializable]
public class DuplicateCardException : PokerAlgoException
{
    public DuplicateCardException() { }
    public DuplicateCardException(string message) : base(message) { }
}