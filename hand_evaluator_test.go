package pokeralgo

import "testing"

func TestGetWinningHandValidation(t *testing.T) {
	deck := NewDeck()
	cards := deck.MustNextCards(4)
	if _, err := GetWinningHand(cards); err == nil {
		t.Fatal("expected error for less than five cards")
	}

	cards = deck.MustNextCards(8)
	if _, err := GetWinningHand(cards); err == nil {
		t.Fatal("expected error for more than seven cards")
	}

	cards = []Card{
		MustCard(5, Spades, false),
		MustCard(5, Spades, false),
		MustCard(6, Spades, false),
		MustCard(7, Spades, false),
		MustCard(8, Spades, false),
		MustCard(9, Spades, false),
		MustCard(10, Spades, false),
	}
	if _, err := GetWinningHand(cards); !IsErrorKind(err, ErrDuplicateCards) {
		t.Fatalf("expected duplicate cards, got %v", err)
	}

	cards = []Card{
		MustCard(1, Spades, false),
		MustCard(5, Spades, false),
		MustCard(6, Spades, false),
		MustCard(7, Spades, false),
		MustCard(8, Spades, false),
		MustCard(9, Spades, false),
		MustCard(10, Spades, false),
	}
	if _, err := GetWinningHand(cards); !IsErrorKind(err, ErrLowAces) {
		t.Fatalf("expected low aces, got %v", err)
	}
}

func TestGetWinningHandReturnsForValidInput(t *testing.T) {
	deck := NewDeck()
	cards := deck.MustNextCards(6)

	if _, err := GetWinningHand(cards); err != nil {
		t.Fatalf("expected valid hand, got %v", err)
	}
}

func TestGetWinningHandForPlayerOverloadEquivalent(t *testing.T) {
	community := []Card{
		MustCard(7, Spades, false),
		MustCard(5, Spades, false),
		MustCard(10, Spades, false),
	}
	holeCards := Pair{First: MustCard(3, Clubs, true), Second: MustCard(10, Diamonds, true)}

	if _, err := GetWinningHandForPlayer(holeCards, community); err != nil {
		t.Fatalf("expected valid hand, got %v", err)
	}
}
