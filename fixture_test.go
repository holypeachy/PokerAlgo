package pokeralgo

import (
	"encoding/json"
	"os"
	"path/filepath"
	"testing"
)

type handEvalFixture struct {
	Description    string
	PlayerCards    Pair
	CommunityCards []Card
	ExpectedHand   WinningHand
}

type algoFixture struct {
	Description      string
	Player1          Pair
	Player2          Pair
	Player3          Pair
	CommunityCards   []Card
	IndicesOfWinners []int
}

func TestHandEvaluatorFixtures(t *testing.T) {
	var fixtures []handEvalFixture
	readJSONFixture(t, filepath.Join("testdata", "HandEvalUnitTests.json"), &fixtures)

	for _, fixture := range fixtures {
		t.Run(fixture.Description, func(t *testing.T) {
			player := NewPlayer("TestPlayer", fixture.PlayerCards.First, fixture.PlayerCards.Second)
			cards := []Card{player.HoleCards.First, player.HoleCards.Second}
			cards = append(cards, fixture.CommunityCards...)
			sortCardsByValue(cards)

			actual, err := GetWinningHand(cards)
			if err != nil {
				t.Fatalf("GetWinningHand: %v", err)
			}

			if actual.Type != fixture.ExpectedHand.Type {
				t.Fatalf("expected hand type %v, got %v", fixture.ExpectedHand.Type, actual.Type)
			}
			assertCardsEqual(t, actual.Cards, fixture.ExpectedHand.Cards)
		})
	}
}

func TestAlgoFixtures(t *testing.T) {
	var fixtures []algoFixture
	readJSONFixture(t, filepath.Join("testdata", "AlgoTests.json"), &fixtures)

	for _, fixture := range fixtures {
		t.Run(fixture.Description, func(t *testing.T) {
			players := []Player{
				NewPlayer("Test Player 1", fixture.Player1.First, fixture.Player1.Second),
				NewPlayer("Test Player 2", fixture.Player2.First, fixture.Player2.Second),
				NewPlayer("Test Player 3", fixture.Player3.First, fixture.Player3.Second),
			}

			winners, err := GetWinners(players, fixture.CommunityCards)
			if err != nil {
				t.Fatalf("GetWinners: %v", err)
			}

			if len(winners) != len(fixture.IndicesOfWinners) {
				t.Fatalf("expected %d winners, got %d", len(fixture.IndicesOfWinners), len(winners))
			}

			for _, expectedIndex := range fixture.IndicesOfWinners {
				if !containsPlayer(winners, players[expectedIndex]) {
					t.Fatalf("expected player index %d to be a winner; winners: %v", expectedIndex, winners)
				}
			}
		})
	}
}

func readJSONFixture(t *testing.T, path string, target any) {
	t.Helper()
	data, err := os.ReadFile(path)
	if err != nil {
		t.Fatalf("read fixture: %v", err)
	}
	if err := json.Unmarshal(data, target); err != nil {
		t.Fatalf("unmarshal fixture: %v", err)
	}
}

func assertCardsEqual(t *testing.T, actual []Card, expected []Card) {
	t.Helper()
	if len(actual) != len(expected) {
		t.Fatalf("expected %d cards, got %d", len(expected), len(actual))
	}
	for i := range actual {
		if !actual[i].Equal(expected[i]) {
			t.Fatalf("card %d: expected %s, got %s", i, expected[i], actual[i])
		}
	}
}

func assertUniqueCards(t *testing.T, cards []Card) {
	t.Helper()
	seen := make(map[[2]int]struct{}, len(cards))
	for _, card := range cards {
		key := [2]int{card.Rank, int(card.Suit)}
		if _, ok := seen[key]; ok {
			t.Fatalf("duplicate card found: %s", card)
		}
		seen[key] = struct{}{}
	}
}
