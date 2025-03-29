namespace PokerAlgo.Testing;

public class CardTests
{
    [Fact]
    public void Card_invalid_rank_on_creation_0()
    {
        Assert.Throws<InvalidCardRankException>(() => new Card(0, CardSuit.Spades, true));
    }

    [Fact]
    public void Card_invalid_rank_on_creation_15()
    {
        Assert.Throws<InvalidCardRankException>(() => new Card(15, CardSuit.Spades, true));
    }

    [Fact]
    public void Card_valid_rank_on_creation()
    {
        new Card(7, CardSuit.Spades, true);
    }
    
}