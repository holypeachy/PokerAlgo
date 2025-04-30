namespace PokerAlgo;
public class Pair
{
    public Card First { get; }
    public Card Second { get; }
    
    public Pair(Card first, Card second)
    {
        this.First = first;
        this.Second = second;
    }

    public override string ToString()
    {
        return $"{First} {Second}";
    }
}