package pokeralgo

import (
	"math"
	"path/filepath"
	"testing"
)

func TestWinningChanceSimValidation(t *testing.T) {
	deck := NewDeck()
	playerCards := Pair{First: deck.MustNextCard(), Second: deck.MustNextCard()}
	communityCards := []Card{deck.MustNextCard(), deck.MustNextCard()}

	if _, err := GetWinningChanceSim(playerCards, communityCards, 2, 500); err == nil {
		t.Fatal("expected error for community less than three")
	}

	communityCards = deck.MustNextCards(6)
	if _, err := GetWinningChanceSim(playerCards, communityCards, 2, 500); err == nil {
		t.Fatal("expected error for community more than five")
	}

	communityCards = deck.MustNextCards(5)
	if _, err := GetWinningChanceSim(playerCards, communityCards, 0, 500); err == nil {
		t.Fatal("expected error for zero opponents")
	}

	if _, err := GetWinningChanceSim(playerCards, communityCards, 4, 10); err == nil {
		t.Fatal("expected error for too few simulations")
	}
}

func TestGetWinningChanceSimReturnsWhenValidInput(t *testing.T) {
	deck := NewDeck()
	playerCards := Pair{First: deck.MustNextCard(), Second: deck.MustNextCard()}
	communityCards := deck.MustNextCards(5)

	if _, err := GetWinningChanceSim(playerCards, communityCards, 4, 500); err != nil {
		t.Fatalf("expected valid sim input, got %v", err)
	}
}

func TestPreFlopSimValidation(t *testing.T) {
	deck := NewDeck()
	playerCards := Pair{First: deck.MustNextCard(), Second: deck.MustNextCard()}

	if _, err := GetWinningChancePreFlopSim(playerCards, 0, 500); err == nil {
		t.Fatal("expected error for zero opponents")
	}

	if _, err := GetWinningChancePreFlopSim(playerCards, 4, 99); err == nil {
		t.Fatal("expected error for too few simulations")
	}
}

func TestGetPreFlopChenKnownValues(t *testing.T) {
	tests := []struct {
		name     string
		cards    Pair
		expected float64
	}{
		{"AKs", Pair{First: MustCard(14, Spades, true), Second: MustCard(13, Spades, true)}, 12},
		{"TTo", Pair{First: MustCard(10, Spades, true), Second: MustCard(10, Hearts, true)}, 10},
		{"57s", Pair{First: MustCard(5, Spades, true), Second: MustCard(7, Spades, true)}, 6},
		{"27o", Pair{First: MustCard(2, Spades, true), Second: MustCard(7, Hearts, true)}, -1},
		{"AAo", Pair{First: MustCard(14, Spades, true), Second: MustCard(14, Hearts, true)}, 20},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			actual, err := GetPreFlopChen(test.cards)
			if err != nil {
				t.Fatalf("GetPreFlopChen: %v", err)
			}
			if actual != test.expected {
				t.Fatalf("expected %v, got %v", test.expected, actual)
			}
		})
	}
}

func TestChanceCalculatorsThrowWhenDuplicateCards(t *testing.T) {
	holeCards := Pair{First: MustCard(2, Spades, true), Second: MustCard(2, Spades, true)}
	communityCards := []Card{
		MustCard(4, Spades, false),
		MustCard(5, Spades, false),
		MustCard(6, Spades, false),
		MustCard(7, Spades, false),
		MustCard(8, Spades, false),
	}

	if _, err := GetWinningChanceSim(holeCards, communityCards, 4, 500); !IsErrorKind(err, ErrDuplicateCards) {
		t.Fatalf("expected duplicate cards, got %v", err)
	}
	if _, err := GetWinningChancePreFlopSim(holeCards, 4, 500); !IsErrorKind(err, ErrDuplicateCards) {
		t.Fatalf("expected duplicate cards, got %v", err)
	}
	if _, err := GetWinningChancePreFlopLookUp(holeCards, 4, NewFolderLoader(preflopPath())); !IsErrorKind(err, ErrDuplicateCards) {
		t.Fatalf("expected duplicate cards, got %v", err)
	}
	if _, err := GetWinningChancePreFlopChen(holeCards); !IsErrorKind(err, ErrDuplicateCards) {
		t.Fatalf("expected duplicate cards, got %v", err)
	}
	if _, err := GetPreFlopChen(holeCards); !IsErrorKind(err, ErrDuplicateCards) {
		t.Fatalf("expected duplicate cards, got %v", err)
	}
}

func TestWinningChancePreFlopLookUpKnownValues(t *testing.T) {
	loader := NewFolderLoader(preflopPath())

	tests := []struct {
		name        string
		cards       Pair
		opponents   int
		expectedWin float64
		expectedTie float64
	}{
		{"AAo 4 opponents", Pair{First: MustCard(14, Spades, true), Second: MustCard(14, Clubs, true)}, 4, 0.557242, 0.005764},
		{"AKs 4 opponents", Pair{First: MustCard(14, Spades, true), Second: MustCard(13, Spades, true)}, 4, 0.343988, 0.019834},
		{"A4o 2 opponents", Pair{First: MustCard(4, Spades, true), Second: MustCard(14, Hearts, true)}, 2, 0.3509, 0.04338},
		{"AKs 1 opponent", Pair{First: MustCard(14, Spades, true), Second: MustCard(13, Spades, true)}, 1, 0.661998, 0.0164},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			actual, err := GetWinningChancePreFlopLookUp(test.cards, test.opponents, loader)
			if err != nil {
				t.Fatalf("lookup: %v", err)
			}
			assertFloatEqual(t, actual.WinChance, test.expectedWin, 0)
			assertFloatEqual(t, actual.TieChance, test.expectedTie, 0)
		})
	}
}

func TestWinningChancePreFlopLookUpSymmetryAndExternalChecks(t *testing.T) {
	loader := NewFolderLoader(preflopPath())

	ako := Pair{First: MustCard(14, Diamonds, true), Second: MustCard(13, Hearts, true)}
	kao := Pair{First: MustCard(13, Diamonds, true), Second: MustCard(14, Hearts, true)}
	winAKo, err := GetWinningChancePreFlopLookUp(ako, 4, loader)
	if err != nil {
		t.Fatalf("lookup AKo: %v", err)
	}
	winKAo, err := GetWinningChancePreFlopLookUp(kao, 4, loader)
	if err != nil {
		t.Fatalf("lookup KAo: %v", err)
	}
	assertFloatEqual(t, winAKo.WinChance, winKAo.WinChance, 0.01)
	assertFloatEqual(t, winAKo.TieChance, winKAo.TieChance, 0.01)

	o27 := Pair{First: MustCard(2, Diamonds, true), Second: MustCard(7, Hearts, true)}
	o72 := Pair{First: MustCard(7, Diamonds, true), Second: MustCard(2, Hearts, true)}
	win27, err := GetWinningChancePreFlopLookUp(o27, 4, loader)
	if err != nil {
		t.Fatalf("lookup 27o: %v", err)
	}
	win72, err := GetWinningChancePreFlopLookUp(o72, 4, loader)
	if err != nil {
		t.Fatalf("lookup 72o: %v", err)
	}
	assertFloatEqual(t, win27.WinChance, win72.WinChance, 0.01)
	assertFloatEqual(t, win27.TieChance, win72.TieChance, 0.01)

	externalChecks := []struct {
		name        string
		cards       Pair
		expectedWin float64
		expectedTie float64
	}{
		{"9As", Pair{First: MustCard(9, Spades, true), Second: MustCard(14, Spades, true)}, 0.2666, 0.0359},
		{"KKo", Pair{First: MustCard(13, Spades, true), Second: MustCard(13, Diamonds, true)}, 0.4953, 0.0065},
		{"27o", Pair{First: MustCard(2, Spades, true), Second: MustCard(7, Diamonds, true)}, 0.0972, 0.0287},
	}

	for _, check := range externalChecks {
		t.Run(check.name, func(t *testing.T) {
			actual, err := GetWinningChancePreFlopLookUp(check.cards, 4, loader)
			if err != nil {
				t.Fatalf("lookup: %v", err)
			}
			assertFloatEqual(t, actual.WinChance, check.expectedWin, 0.01)
			assertFloatEqual(t, actual.TieChance, check.expectedTie, 0.01)
		})
	}
}

func TestWinningChanceSimParallelAndWinningChanceSimSymmetric(t *testing.T) {
	holeCards := Pair{First: MustCard(14, Spades, true), Second: MustCard(14, Clubs, true)}
	community := []Card{
		MustCard(7, Hearts, false),
		MustCard(8, Hearts, false),
		MustCard(9, Hearts, false),
		MustCard(12, Hearts, false),
		MustCard(8, Diamonds, false),
	}

	simChance, err := GetWinningChanceSim(holeCards, community, 4, 500_000)
	if err != nil {
		t.Fatalf("sim: %v", err)
	}
	parallelChance, err := GetWinningChanceSimParallel(holeCards, community, 4, 500_000)
	if err != nil {
		t.Fatalf("parallel sim: %v", err)
	}

	assertFloatEqual(t, simChance.WinChance, parallelChance.WinChance, 0.001)
	assertFloatEqual(t, simChance.TieChance, parallelChance.TieChance, 0.001)
}

func TestWinningChancePreFlopSimParallelAndWinningChancePreFlopSimSymmetric(t *testing.T) {
	holeCards := Pair{First: MustCard(14, Spades, true), Second: MustCard(14, Clubs, true)}

	simChance, err := GetWinningChancePreFlopSim(holeCards, 4, 500_000)
	if err != nil {
		t.Fatalf("preflop sim: %v", err)
	}
	parallelChance, err := GetWinningChancePreFlopSimParallel(holeCards, 4, 500_000)
	if err != nil {
		t.Fatalf("parallel preflop sim: %v", err)
	}

	assertFloatEqual(t, simChance.WinChance, parallelChance.WinChance, 0.001)
	assertFloatEqual(t, simChance.TieChance, parallelChance.TieChance, 0.001)
}

func TestWinningChanceSimProbabilitiesInRange(t *testing.T) {
	deck := NewDeck()
	holeCards := Pair{First: deck.MustNextCard(), Second: deck.MustNextCard()}
	communityCards := deck.MustNextCards(5)

	chance, err := GetWinningChanceSim(holeCards, communityCards, 4, 100)
	if err != nil {
		t.Fatalf("sim: %v", err)
	}
	assertChanceInRange(t, chance)
}

func TestWinningChancePreFlopSimProbabilitiesInRange(t *testing.T) {
	deck := NewDeck()
	holeCards := Pair{First: deck.MustNextCard(), Second: deck.MustNextCard()}

	chance, err := GetWinningChancePreFlopSim(holeCards, 4, 100)
	if err != nil {
		t.Fatalf("preflop sim: %v", err)
	}
	assertChanceInRange(t, chance)
}

func TestWinningChancePreFlopLookUpThrowsWhenTooManyOpponents(t *testing.T) {
	_, err := GetWinningChancePreFlopLookUp(
		Pair{First: MustCard(14, Spades, true), Second: MustCard(13, Spades, true)},
		10,
		NewFolderLoader(preflopPath()),
	)
	if err == nil {
		t.Fatal("expected missing opponent data error")
	}
}

func preflopPath() string {
	return filepath.Join("resources", "preflop_data")
}

func assertFloatEqual(t *testing.T, actual float64, expected float64, tolerance float64) {
	t.Helper()
	if math.Abs(actual-expected) > tolerance {
		t.Fatalf("expected %v +/- %v, got %v", expected, tolerance, actual)
	}
}

func assertChanceInRange(t *testing.T, chance Chance) {
	t.Helper()
	if chance.WinChance < 0 || chance.WinChance > 1 {
		t.Fatalf("win chance out of range: %v", chance.WinChance)
	}
	if chance.TieChance < 0 || chance.TieChance > 1 {
		t.Fatalf("tie chance out of range: %v", chance.TieChance)
	}
	if chance.WinChance+chance.TieChance > 1 {
		t.Fatalf("win+tie out of range: %v", chance.WinChance+chance.TieChance)
	}
}
