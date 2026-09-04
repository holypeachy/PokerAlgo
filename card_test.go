package pokeralgo

import "testing"

func TestNewCardValidation(t *testing.T) {
	if _, err := NewCard(0, Spades, true); !IsErrorKind(err, ErrInvalidCardRank) {
		t.Fatalf("expected invalid card rank for 0, got %v", err)
	}

	if _, err := NewCard(15, Spades, true); !IsErrorKind(err, ErrInvalidCardRank) {
		t.Fatalf("expected invalid card rank for 15, got %v", err)
	}

	if _, err := NewCard(7, Spades, true); err != nil {
		t.Fatalf("expected valid card, got %v", err)
	}
}
