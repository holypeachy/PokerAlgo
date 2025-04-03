namespace PokerAlgo.Testing;

public class AlgoTests
{
    [Fact]
    public void GetWinners_less_than_2_players_throws_error()
    {
        Deck deck = new();
        List<Player> players = new()
        {
            new Player("Player 1", deck.NextCard(), deck.NextCard()),
        };

        List<Card> communityCards = deck.NextCards(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => Algo.GetWinners(players, communityCards));
    }

    [Fact]
    public void GetWinners_less_than_3_community_throws_error()
    {
        Deck deck = new();
        List<Player> players = new()
        {
            new Player("Player 1", deck.NextCard(), deck.NextCard()),
            new Player("Player 2", deck.NextCard(), deck.NextCard()),
        };

        List<Card> communityCards = deck.NextCards(2);

        Assert.Throws<ArgumentOutOfRangeException>(() => Algo.GetWinners(players, communityCards));
    }

    [Fact]
    public void GetWinners_more_than_5_community()
    {
        Deck deck = new();
        List<Player> players = new()
        {
            new Player("Player 1", deck.NextCard(), deck.NextCard()),
            new Player("Player 2", deck.NextCard(), deck.NextCard()),
        };

        List<Card> communityCards = deck.NextCards(6);

        Assert.Throws<ArgumentOutOfRangeException>(() => Algo.GetWinners(players, communityCards));
    }

    [Fact]
    public void GetWinners_no_error()
    {
        Deck deck = new();
        List<Player> players = new()
        {
            new Player("Player 1", deck.NextCard(), deck.NextCard()),
            new Player("Player 2", deck.NextCard(), deck.NextCard()),
        };

        List<Card> communityCards = deck.NextCards(5);

        Algo.GetWinners(players, communityCards);
    }

}