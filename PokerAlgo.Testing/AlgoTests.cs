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

    [Fact]
    public void GetWinners_duplicate_cards_throws_error()
    {
        Pair pair1 = new(new Card(5, CardSuit.Spades, true), new Card(5, CardSuit.Spades, true));

        Pair pair2 = new(new Card(5, CardSuit.Diamonds, true), new Card(8, CardSuit.Spades, true));
        Pair pair3 = new(new Card(9, CardSuit.Spades, true), new Card(11, CardSuit.Spades, true));
        List<Player> players = new()
        {
            new Player("Player 1", pair1.First, pair1.Second),
            new Player("Player 2", pair2.First, pair2.Second),
            new Player("Player 3", pair3.First, pair3.Second)
        };

        List<Card> communityCards = new()
        {
            new Card(10, CardSuit.Diamonds, false),
            new Card(14, CardSuit.Diamonds, false),
            new Card(7, CardSuit.Diamonds, false),
            new Card(9, CardSuit.Diamonds, false),
            new Card(3, CardSuit.Diamonds, false),
        };

        Assert.Throws<DuplicateCardException>(() => Algo.GetWinners(players, communityCards));
    }

    [Fact]
    public void GetWinners_low_aces_throws_error()
    {
        Pair pair1 = new(new Card(1, CardSuit.Spades, true), new Card(3, CardSuit.Spades, true));

        Pair pair2 = new(new Card(5, CardSuit.Spades, true), new Card(8, CardSuit.Spades, true));
        Pair pair3 = new(new Card(9, CardSuit.Spades, true), new Card(11, CardSuit.Spades, true));

        List<Player> players = new()
        {
            new Player("Player 1", pair1.First, pair1.Second),
            new Player("Player 2", pair2.First, pair2.Second),
            new Player("Player 3", pair3.First, pair3.Second)
        };

        List<Card> communityCards = new()
        {
            new Card(14, CardSuit.Diamonds, false),
            new Card(10, CardSuit.Diamonds, false),
            new Card(9, CardSuit.Diamonds, false),
            new Card(7, CardSuit.Diamonds, false),
            new Card(3, CardSuit.Diamonds, false),
        };

        Assert.Throws<LowAcesException>(() => Algo.GetWinners(players, communityCards));
    }

    
}