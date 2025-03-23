namespace PokerAlgo.Testing;

public class UnitTests
{
    [Fact]
    public void Algo_GetWinners_less_than_2_players()
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

        Assert.Throws<Exception>(() => Algo.GetWinners(players, communityCards));
    }

    [Fact]
    public void Algo_GetWinners_less_than_3_community()
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

        Assert.Throws<Exception>(() => Algo.GetWinners(players, communityCards));
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

    [Fact]
    public void HandEvaluator_GetWinningHand_less_than_5_cards()
    {
        Deck deck = new();

        List<Card> cards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        HandEvaluator handEvaluator = new();
        Assert.Throws<Exception>(() => handEvaluator.GetWinningHand(cards));
    }

    [Fact]
    public void HandEvaluator_GetWinningHand_more_than_7_cards()
    {
        Deck deck = new();

        List<Card> cards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        HandEvaluator handEvaluator = new();
        Assert.Throws<Exception>(() => handEvaluator.GetWinningHand(cards));
    }

    [Fact]
    public void HandEvaluator_GetWinningHand_no_error()
    {
        Deck deck = new();

        List<Card> cards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        HandEvaluator handEvaluator = new();
        handEvaluator.GetWinningHand(cards);
    }
    
}