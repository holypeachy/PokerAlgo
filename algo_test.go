package pokeralgo

import "testing"

func TestGetWinnersValidation(t *testing.T) {
	deck := NewDeck()
	players := []Player{
		NewPlayer("Player 1", deck.MustNextCard(), deck.MustNextCard()),
	}
	communityCards := deck.MustNextCards(5)
	if _, err := GetWinners(players, communityCards); err == nil {
		t.Fatal("expected error for less than two players")
	}

	deck = NewDeck()
	players = []Player{
		NewPlayer("Player 1", deck.MustNextCard(), deck.MustNextCard()),
		NewPlayer("Player 2", deck.MustNextCard(), deck.MustNextCard()),
	}
	communityCards = deck.MustNextCards(2)
	if _, err := GetWinners(players, communityCards); err == nil {
		t.Fatal("expected error for less than five community cards")
	}

	communityCards = deck.MustNextCards(6)
	if _, err := GetWinners(players, communityCards); err == nil {
		t.Fatal("expected error for more than five community cards")
	}
}

func TestGetWinnersReturnsForValidInput(t *testing.T) {
	deck := NewDeck()
	players := []Player{
		NewPlayer("Player 1", deck.MustNextCard(), deck.MustNextCard()),
		NewPlayer("Player 2", deck.MustNextCard(), deck.MustNextCard()),
	}
	communityCards := deck.MustNextCards(5)

	if _, err := GetWinners(players, communityCards); err != nil {
		t.Fatalf("expected valid winners, got %v", err)
	}
}

func TestGetWinnersThrowsWhenDuplicateCardsExist(t *testing.T) {
	players := []Player{
		NewPlayer("Player 1", MustCard(5, Spades, true), MustCard(5, Spades, true)),
		NewPlayer("Player 2", MustCard(5, Diamonds, true), MustCard(8, Spades, true)),
		NewPlayer("Player 3", MustCard(9, Spades, true), MustCard(11, Spades, true)),
	}
	communityCards := []Card{
		MustCard(10, Diamonds, false),
		MustCard(14, Diamonds, false),
		MustCard(7, Diamonds, false),
		MustCard(9, Diamonds, false),
		MustCard(3, Diamonds, false),
	}

	if _, err := GetWinners(players, communityCards); !IsErrorKind(err, ErrDuplicateCards) {
		t.Fatalf("expected duplicate cards, got %v", err)
	}
}

func TestGetWinnersThrowsWhenLowAceInput(t *testing.T) {
	players := []Player{
		NewPlayer("Player 1", MustCard(1, Spades, true), MustCard(3, Spades, true)),
		NewPlayer("Player 2", MustCard(5, Spades, true), MustCard(8, Spades, true)),
		NewPlayer("Player 3", MustCard(9, Spades, true), MustCard(11, Spades, true)),
	}
	communityCards := []Card{
		MustCard(14, Diamonds, false),
		MustCard(10, Diamonds, false),
		MustCard(9, Diamonds, false),
		MustCard(7, Diamonds, false),
		MustCard(3, Diamonds, false),
	}

	if _, err := GetWinners(players, communityCards); !IsErrorKind(err, ErrLowAces) {
		t.Fatalf("expected low ace, got %v", err)
	}
}
