namespace PokerAlgo.Testing;

public class HandEvaluatorTests
{
    [Fact]
    public void GetWinningHand_Throws_When_LessThanFiveCards() // 2 player cards + 3 community cards
    {
        Deck deck = new();

        List<Card> cards = deck.NextCards(4);

        HandEvaluator handEvaluator = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => handEvaluator.GetWinningHand(cards));
    }

    [Fact]
    public void GetWinningHand_Throws_When_MoreThanSevenCards() // 2 player cards + 5 community cards
    {
        Deck deck = new();

        List<Card> cards = deck.NextCards(8);

        HandEvaluator handEvaluator = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => handEvaluator.GetWinningHand(cards));
    }

    [Fact]
    public void GetWinningHand_Returns_When_ValidInput()
    {
        Deck deck = new();

        List<Card> cards = deck.NextCards(6);

        HandEvaluator handEvaluator = new();
        handEvaluator.GetWinningHand(cards);
    }


    [Fact]
    public void GetWinningHand_Throws_When_DuplicateCards()
    {
        HandEvaluator handEvaluator = new();

        List<Card> cards = new()
        {
            new Card(5, CardSuit.Spades, false),
            new Card(5, CardSuit.Spades, false),
            new Card(6, CardSuit.Spades, false),
            new Card(7, CardSuit.Spades, false),
            new Card(8, CardSuit.Spades, false),
            new Card(9, CardSuit.Spades, false),
            new Card(10, CardSuit.Spades, false),
        };

        Assert.Throws<DuplicateCardException>(() => handEvaluator.GetWinningHand(cards));
    }

    [Fact]
    public void GetWinningHand_Throws_When_LowAceDetected()
    {
        HandEvaluator handEvaluator = new();

        List<Card> cards = new()
        {
            new Card(1, CardSuit.Spades, false),
            new Card(5, CardSuit.Spades, false),
            new Card(6, CardSuit.Spades, false),
            new Card(7, CardSuit.Spades, false),
            new Card(8, CardSuit.Spades, false),
            new Card(9, CardSuit.Spades, false),
            new Card(10, CardSuit.Spades, false),
        };

        Assert.Throws<LowAcesException>(() => handEvaluator.GetWinningHand(cards));
    }
}