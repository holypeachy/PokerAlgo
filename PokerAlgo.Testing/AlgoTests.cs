namespace PokerAlgo.Testing;

public class AlgoTests
{
    [Fact]
    public void Algo_GetWinners_less_than_2_players_throws_error()
    {
        Deck deck = new();
        List<Player> players = new()
        {
            new Player("Player 1", deck.NextCard(), deck.NextCard()),
        };

        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => Algo.GetWinners(players, communityCards));
    }

    [Fact]
    public void Algo_GetWinners_less_than_3_community_throws_error()
    {
        Deck deck = new();
        List<Player> players = new()
        {
            new Player("Player 1", deck.NextCard(), deck.NextCard()),
            new Player("Player 2", deck.NextCard(), deck.NextCard()),
        };

        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => Algo.GetWinners(players, communityCards));
    }

    [Fact]
    public void Algo_GetWinners_more_than_5_community()
    {
        Deck deck = new();
        List<Player> players = new()
        {
            new Player("Player 1", deck.NextCard(), deck.NextCard()),
            new Player("Player 2", deck.NextCard(), deck.NextCard()),
        };

        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => Algo.GetWinners(players, communityCards));
    }

    [Fact]
    public void Algo_GetWinners_no_error()
    {
        Deck deck = new();
        List<Player> players = new()
        {
            new Player("Player 1", deck.NextCard(), deck.NextCard()),
            new Player("Player 2", deck.NextCard(), deck.NextCard()),
        };

        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        Algo.GetWinners(players, communityCards);
    }

}