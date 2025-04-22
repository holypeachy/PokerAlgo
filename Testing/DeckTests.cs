namespace Testing;

public class DeckTests
{
    [Fact]
    public void Deck_ShouldContainUniqueCards_AfterResetAndRemove()
    {
        Deck deck = new();
        List<Card> cardsToRemove = deck.NextCards(20);

        deck.ResetDeck();
        deck.RemoveCards(cardsToRemove);
        deck.NextCard();
        deck.NextCards(20);
        deck.RemoveCards(cardsToRemove);

        List<Card> newUniqueCards = new();
        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
        {
            for (int rank = 2; rank <= 14; rank++)
            {
                newUniqueCards.Add(new Card(rank, suit, false));
            }
        }

        List<Card> deckCards = deck.GetCopyOfListOfCards();

        deckCards.Count.Should().Be(52);
        deckCards.Should().OnlyHaveUniqueItems();
        deckCards.Should().BeEquivalentTo(newUniqueCards);
    }

    [Fact]
    public void NextCard_Throws_When_DeckIsEmpty()
    {
        // Given
        Deck deck = new();
        // When
        for (int i = 0; i < 52; i++)
        {
            deck.NextCard();
        }
        // Then
        Assert.Throws<DeckEmptyException>(() => deck.NextCard());
    }

    [Fact]
    public void NextCards_Throws_When_DeckIsEmpty()
    {
        // Given
        Deck deck = new();
        // When
        deck.NextCards(52);
        // Then
        Assert.Throws<DeckEmptyException>(() => deck.NextCards(1));
    }

    [Fact]
    public void NextCards_Succeeds_When_ExactlyOneCardLeft()
    {
        // Given
        Deck deck = new();
        // When
        deck.NextCards(51);
        // Then
        deck.NextCards(1);
    }

    [Fact]
    public void NextCards_Throws_When_NotEnoughCardsLeft()
    {
        // Given
        Deck deck = new();
        // When
        deck.NextCards(50);
        // Then
        deck.NextCardIndex.Should().Be(50);
        Assert.Throws<NotEnoughCardsInDeckException>(() => deck.NextCards(3));
    }

    [Fact]
    public void NextCards_Throws_When_CountIsZeroOrNegative()
    {
        // Given
        Deck deck = new();
        // Then
        Assert.Throws<ArgumentOutOfRangeException>(() => deck.NextCards(0));
    }

    [Fact]
    public void RemoveCards_DoesNotAdvanceIndex_When_SkippingRemovedCards()
    {
        Deck deck = new();
        List<Card> copyOfDeck = deck.GetCopyOfListOfCards();

        List<Card> cardsToRemoveBefore = deck.NextCards(10);
        deck.NextCardIndex.Should().Be(10);

        deck.RemoveCards(cardsToRemoveBefore);
        deck.NextCardIndex.Should().Be(10);

        List<Card> cardsToRemove5After = copyOfDeck.GetRange(5, 10);
        deck.RemoveCards(cardsToRemove5After);
        deck.NextCardIndex.Should().Be(15);
    }

    [Fact]
    public void NextCards_MovesIndexCorrectly_When_15CardsDrawn()
    {
        // Given
        Deck deck = new();
        // When
        deck.NextCards(15);
        // Then
        deck.NextCardIndex.Should().Be(15);
    }

    
}