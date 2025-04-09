namespace PokerAlgo;

/// <summary>
/// The base class for all exceptions thrown by the PokerAlgo library.
/// </summary>
[Serializable]
public class PokerAlgoException : Exception
{
    public PokerAlgoException() { }
    public PokerAlgoException(string message) : base(message) { }
}