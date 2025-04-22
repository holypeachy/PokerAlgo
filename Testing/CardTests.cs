namespace PokerAlgo.Testing;

public class CardTests
{
    [Fact]
    public void Card_Throws_When_RankIsZero()
    {
        Assert.Throws<InvalidCardRankException>(() => new Card(0, CardSuit.Spades, true));
    }

    [Fact]
    public void Card_Throws_When_RankIsAbove14()
    {
        Assert.Throws<InvalidCardRankException>(() => new Card(15, CardSuit.Spades, true));
    }

    [Fact]
    public void Card_CreatesSuccessfully_WhenRankIsValid()
    {
        new Card(7, CardSuit.Spades, true);
    }
    
}