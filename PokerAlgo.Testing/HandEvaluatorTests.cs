namespace PokerAlgo.Testing;

public class HandEvaluatorTests
{
    [Fact]
    public void GetWinningHand_no_less_than_5_cards() // 2 player cards + 3 community cards
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
        Assert.Throws<ArgumentOutOfRangeException>(() => handEvaluator.GetWinningHand(cards));
    }

    [Fact]
    public void GetWinningHand_no_more_than_7_cards() // 2 player cards + 5 community cards
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
        Assert.Throws<ArgumentOutOfRangeException>(() => handEvaluator.GetWinningHand(cards));
    }

    [Fact]
    public void GetWinningHand_no_error()
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