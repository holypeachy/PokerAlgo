package pokeralgo

import "testing"

func TestDeckShouldContainUniqueCardsAfterResetAndRemove(t *testing.T) {
	deck := NewDeck()
	cardsToRemove := deck.MustNextCards(20)

	deck.ResetDeck()
	if err := deck.RemoveCards(cardsToRemove); err != nil {
		t.Fatalf("remove cards: %v", err)
	}
	deck.MustNextCard()
	deck.MustNextCards(20)
	if err := deck.RemoveCards(cardsToRemove); err != nil {
		t.Fatalf("remove cards again: %v", err)
	}

	deckCards := deck.CopyOfCards()
	if len(deckCards) != 52 {
		t.Fatalf("expected 52 cards, got %d", len(deckCards))
	}
	assertUniqueCards(t, deckCards)
}

func TestNextCardThrowsWhenDeckIsEmpty(t *testing.T) {
	deck := NewDeck()
	for i := 0; i < 52; i++ {
		deck.MustNextCard()
	}

	if _, err := deck.NextCard(); !IsErrorKind(err, ErrDeckEmpty) {
		t.Fatalf("expected deck empty, got %v", err)
	}
}

func TestNextCardsThrowsWhenDeckIsEmpty(t *testing.T) {
	deck := NewDeck()
	deck.MustNextCards(52)

	if _, err := deck.NextCards(1); !IsErrorKind(err, ErrDeckEmpty) {
		t.Fatalf("expected deck empty, got %v", err)
	}
}

func TestNextCardsSucceedsWhenExactlyOneCardLeft(t *testing.T) {
	deck := NewDeck()
	deck.MustNextCards(51)

	if _, err := deck.NextCards(1); err != nil {
		t.Fatalf("expected one card draw to succeed, got %v", err)
	}
}

func TestNextCardsThrowsWhenNotEnoughCardsLeft(t *testing.T) {
	deck := NewDeck()
	deck.MustNextCards(50)

	if deck.NextCardIndex != 50 {
		t.Fatalf("expected next card index 50, got %d", deck.NextCardIndex)
	}
	if _, err := deck.NextCards(3); !IsErrorKind(err, ErrNotEnoughCards) {
		t.Fatalf("expected not enough cards, got %v", err)
	}
}

func TestNextCardsThrowsWhenCountIsZeroOrNegative(t *testing.T) {
	deck := NewDeck()
	if _, err := deck.NextCards(0); err == nil {
		t.Fatal("expected error for zero cards")
	}
}

func TestRemoveCardsMovesIndexLikeCSharpDeck(t *testing.T) {
	deck := NewDeck()
	copyOfDeck := deck.CopyOfCards()

	cardsToRemoveBefore := deck.MustNextCards(10)
	if deck.NextCardIndex != 10 {
		t.Fatalf("expected next card index 10, got %d", deck.NextCardIndex)
	}

	if err := deck.RemoveCards(cardsToRemoveBefore); err != nil {
		t.Fatalf("remove cards before: %v", err)
	}
	if deck.NextCardIndex != 10 {
		t.Fatalf("expected next card index to stay 10, got %d", deck.NextCardIndex)
	}

	cardsToRemove5After := copyOfDeck[5:15]
	if err := deck.RemoveCards(cardsToRemove5After); err != nil {
		t.Fatalf("remove cards after: %v", err)
	}
	if deck.NextCardIndex != 15 {
		t.Fatalf("expected next card index 15, got %d", deck.NextCardIndex)
	}
}

func TestNextCardsMovesIndexCorrectly(t *testing.T) {
	deck := NewDeck()
	deck.MustNextCards(15)

	if deck.NextCardIndex != 15 {
		t.Fatalf("expected next card index 15, got %d", deck.NextCardIndex)
	}
}

func TestSeedGeneratesSameDeckOrder(t *testing.T) {
	const seed int64 = 123456

	deck := NewDeck()
	deck.ResetDeckWithSeed(seed)
	firstOrder := deck.CopyOfCards()
	deck.ResetDeckWithSeed(seed)
	secondOrder := deck.CopyOfCards()

	assertCardsEqual(t, firstOrder, secondOrder)

	firstDeck := NewDeckWithSeed(seed)
	secondDeck := NewDeckWithSeed(seed)
	assertCardsEqual(t, firstDeck.CopyOfCards(), secondDeck.CopyOfCards())
}
