namespace PokerAlgo;
public class Pair
{
    public Card First { get; }
    public Card Second { get; }
    
    public Pair(Card First, Card Second)
    {
        this.First = First;
        this.Second = Second;
    }

    public override string ToString()
    {
        return $"{First} {Second}";
    }
}