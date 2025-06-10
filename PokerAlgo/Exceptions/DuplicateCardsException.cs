namespace PokerAlgo;
/// <summary>
/// The exception that is thrown when duplicate cards are detected in a context that requires all cards to be unique.
/// </summary>
[Serializable]
public class DuplicateCardException : PokerAlgoException
{
    public DuplicateCardException() { }
    public DuplicateCardException(string message) : base(message) { }
}