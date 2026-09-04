package pokeralgo

import "fmt"

type CardSuit int

const (
	Spades CardSuit = iota
	Clubs
	Hearts
	Diamonds
)

func (s CardSuit) String() string {
	switch s {
	case Spades:
		return "Spades"
	case Clubs:
		return "Clubs"
	case Hearts:
		return "Hearts"
	case Diamonds:
		return "Diamonds"
	default:
		return fmt.Sprintf("CardSuit(%d)", s)
	}
}

type Card struct {
	Rank         int
	Suit         CardSuit
	IsPlayerCard bool
}

func NewCard(rank int, suit CardSuit, isPlayerCard bool) (Card, error) {
	if rank < 1 || rank > 14 {
		return Card{}, newError(ErrInvalidCardRank, fmt.Sprintf("rank value passed: %d. Values must be 1-14. Both 1 and 14 represent Ace.", rank))
	}

	return Card{Rank: rank, Suit: suit, IsPlayerCard: isPlayerCard}, nil
}

func MustCard(rank int, suit CardSuit, isPlayerCard bool) Card {
	card, err := NewCard(rank, suit, isPlayerCard)
	if err != nil {
		panic(err)
	}
	return card
}

// Does not take into consideration IsPlayerCard
func (c Card) Equal(other Card) bool {
	return c.Rank == other.Rank && c.Suit == other.Suit
}

func (c Card) String() string {
	cardPrintLookUp := map[int]string{
		1: "A", 2: "2", 3: "3", 4: "4", 5: "5", 6: "6", 7: "7",
		8: "8", 9: "9", 10: "T", 11: "J", 12: "Q", 13: "K", 14: "A",
	}

	playerMark := ""
	if c.IsPlayerCard {
		playerMark = "*"
	}

	return fmt.Sprintf("[%s,%s]%s", cardPrintLookUp[c.Rank], c.Suit, playerMark)
}
