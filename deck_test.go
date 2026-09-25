package pokeralgo

import "testing"

func TestDeckShouldContainUniqueCardsAfterResetAndRemove(t *testing.T) {
	deck := NewDeck()
	cardsToRemove := deck.MustDrawN(20)

	deck.Reset()
	if err := deck.Exclude(cardsToRemove); err != nil {
		t.Fatalf("remove cards: %v", err)
	}
	deck.MustDraw()
	deck.MustDrawN(20)
	if err := deck.Exclude(cardsToRemove); err != nil {
		t.Fatalf("remove cards again: %v", err)
	}

	deckCards := deck.Cards()
	if len(deckCards) != 52 {
		t.Fatalf("expected 52 cards, got %d", len(deckCards))
	}
	assertUniqueCards(t, deckCards)
}

func TestNextCardThrowsWhenDeckIsEmpty(t *testing.T) {
	deck := NewDeck()
	for i := 0; i < 52; i++ {
		deck.MustDraw()
	}

	if _, err := deck.Draw(); !IsErrorKind(err, ErrDeckEmpty) {
		t.Fatalf("expected deck empty, got %v", err)
	}
}

func TestNextCardsThrowsWhenDeckIsEmpty(t *testing.T) {
	deck := NewDeck()
	deck.MustDrawN(52)

	if _, err := deck.DrawN(1); !IsErrorKind(err, ErrDeckEmpty) {
		t.Fatalf("expected deck empty, got %v", err)
	}
}

func TestNextCardsSucceedsWhenExactlyOneCardLeft(t *testing.T) {
	deck := NewDeck()
	deck.MustDrawN(51)

	if _, err := deck.DrawN(1); err != nil {
		t.Fatalf("expected one card draw to succeed, got %v", err)
	}
}

func TestNextCardsThrowsWhenNotEnoughCardsLeft(t *testing.T) {
	deck := NewDeck()
	deck.MustDrawN(50)

	if deck.Remaining() != 2 {
		t.Fatalf("expected 2 cards remaining, got %d", deck.Remaining())
	}
	if _, err := deck.DrawN(3); !IsErrorKind(err, ErrNotEnoughCards) {
		t.Fatalf("expected not enough cards, got %v", err)
	}
}

func TestNextCardsThrowsWhenCountIsZeroOrNegative(t *testing.T) {
	deck := NewDeck()
	if _, err := deck.DrawN(0); err == nil {
		t.Fatal("expected error for zero cards")
	}
}

func TestRemoveCardsMovesIndexLikeCSharpDeck(t *testing.T) {
	deck := NewDeck()
	copyOfDeck := deck.Cards()

	cardsToRemoveBefore := deck.MustDrawN(10)
	if deck.Remaining() != 42 {
		t.Fatalf("expected 42 cards remaining, got %d", deck.Remaining())
	}

	if err := deck.Exclude(cardsToRemoveBefore); err != nil {
		t.Fatalf("remove cards before: %v", err)
	}
	if deck.Remaining() != 42 {
		t.Fatalf("expected 42 cards remaining, got %d", deck.Remaining())
	}

	cardsToRemove5After := copyOfDeck[5:15]
	if err := deck.Exclude(cardsToRemove5After); err != nil {
		t.Fatalf("remove cards after: %v", err)
	}
	if deck.Remaining() != 37 {
		t.Fatalf("expected 37 cards remaining, got %d", deck.Remaining())
	}
}

func TestNextCardsMovesIndexCorrectly(t *testing.T) {
	deck := NewDeck()
	deck.MustDrawN(15)

	if deck.Remaining() != 37 {
		t.Fatalf("expected 37 cards remaining, got %d", deck.Remaining())
	}
}

func TestSeedGeneratesSameDeckOrder(t *testing.T) {
	const seed int64 = 123456

	deck1 := NewDeckWithSeed(seed)
	deck2 := NewDeckWithSeed(seed)

	assertCardsEqual(t, deck1.cards, deck2.cards)

	firstDeck := NewDeckWithSeed(seed)
	secondDeck := NewDeckWithSeed(seed)
	assertCardsEqual(t, firstDeck.Cards(), secondDeck.Cards())
}
