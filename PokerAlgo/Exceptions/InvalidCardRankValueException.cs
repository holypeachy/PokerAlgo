namespace PokerAlgo;
/// <summary>
/// The exception that is thrown when a card is initialized with an invalid rank value.
/// Valid ranks range from 1 to 14, where 1 and 14 represent an Ace.
/// </summary>
[Serializable]
public class InvalidCardRankException : PokerAlgoException
{
    public InvalidCardRankException() { }
    public InvalidCardRankException(string message) : base(message) { }
}