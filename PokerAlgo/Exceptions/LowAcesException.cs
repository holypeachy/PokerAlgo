namespace PokerAlgo;
/// <summary>
/// The exception that is thrown when a low Ace (rank 1) is encountered in a context where only high Aces (rank 14) are valid.
/// Aces should always be instantiated with a rank of 14 when passed to the PokerAlgo engine.
/// </summary>
public class LowAcesException : PokerAlgoException
{
    public LowAcesException() { }
    public LowAcesException(string message) : base(message) { }
}