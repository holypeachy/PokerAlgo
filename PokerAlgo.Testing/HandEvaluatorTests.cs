namespace PokerAlgo.Testing;

public class HandEvaluatorTests
{
    [Fact]
    public void GetWinningHand_no_less_than_5_cards() // 2 player cards + 3 community cards
    {
        Deck deck = new();

        List<Card> cards = deck.NextCards(4);

        HandEvaluator handEvaluator = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => handEvaluator.GetWinningHand(cards));
    }

    [Fact]
    public void GetWinningHand_no_more_than_7_cards() // 2 player cards + 5 community cards
    {
        Deck deck = new();

        List<Card> cards = deck.NextCards(8);

        HandEvaluator handEvaluator = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => handEvaluator.GetWinningHand(cards));
    }

    [Fact]
    public void GetWinningHand_no_error()
    {
        Deck deck = new();

        List<Card> cards = deck.NextCards(6);

        HandEvaluator handEvaluator = new();
        handEvaluator.GetWinningHand(cards);
    }

}