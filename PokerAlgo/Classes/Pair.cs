namespace PokerAlgo;
public class Pair<T1, T2>
{
    public T1 First { get; }
    public T2 Second { get; }
    
    public Pair(T1 First, T2 Second)
    {
        this.First = First;
        this.Second = Second;
    }

    public override string ToString()
    {
        return $"{First} {Second}";
    }
}