namespace PokerAlgo;
/// <summary>
/// The exception that is thrown when an unexpected internal error occurs within the PokerAlgo engine.
/// Used for cases that should never happen under normal conditions.
/// </summary>
[Serializable]
public class InternalPokerAlgoException : PokerAlgoException
{
    public InternalPokerAlgoException() { }
    public InternalPokerAlgoException(string message) : base(message) { }
}