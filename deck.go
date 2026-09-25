package pokeralgo

import (
	"fmt"
	"math/rand"
	"slices"
	"time"
)

type Deck struct {
	cards         []Card
	rand          *rand.Rand
	nextCardIndex int
	seed          int64
}

func NewDeck() *Deck {
	seed := time.Now().UnixNano()
	return NewDeckWithSeed(seed)
}

func NewDeckWithSeed(seed int64) *Deck {
	deck := &Deck{
		cards: make([]Card, 0, 52),
		rand:  rand.New(rand.NewSource(seed)),
		seed:  seed,
	}

	deck.create()
	deck.shuffle()
	return deck
}

func (d *Deck) create() {
	for _, suit := range []Suit{Spades, Clubs, Hearts, Diamonds} {
		for rank := 2; rank <= 14; rank++ {
			d.cards = append(d.cards, MustCard(rank, suit, false))
		}
	}
}

func (d *Deck) shuffle() {
	for currentIndex := 0; currentIndex < len(d.cards); currentIndex++ {
		targetIndex := currentIndex + d.rand.Intn(len(d.cards)-currentIndex)
		d.cards[currentIndex], d.cards[targetIndex] = d.cards[targetIndex], d.cards[currentIndex]
	}
}

func (d *Deck) Reset() {
	d.nextCardIndex = 0
	d.shuffle()
}

func (d *Deck) resetOrder() {
	d.cards = d.cards[:0]
	d.create()
}

// Returns the first card, and then removes it from the deck
func (d *Deck) Draw() (Card, error) {
	if d.nextCardIndex >= len(d.cards) {
		return Card{}, newError(ErrDeckEmpty, "no more cards in the deck")
	}

	card := d.cards[d.nextCardIndex]
	d.nextCardIndex++
	return card, nil
}

func (d *Deck) MustDraw() Card {
	card, err := d.Draw()
	if err != nil {
		panic(err)
	}
	return card
}

func (d *Deck) DrawN(numberOfCards int) ([]Card, error) {
	if numberOfCards < 1 {
		return nil, newError(ErrInvalidArgument, "numberOfCards must be greater than 0")
	}
	if d.nextCardIndex >= len(d.cards) {
		return nil, newError(ErrDeckEmpty, "no more cards in the deck")
	}
	if d.nextCardIndex+numberOfCards > len(d.cards) {
		return nil, newError(ErrNotEnoughCards, fmt.Sprintf("cards left: %d. Cards requested: %d", len(d.cards)-d.nextCardIndex, numberOfCards))
	}

	cards := slices.Clone(d.cards[d.nextCardIndex : d.nextCardIndex+numberOfCards])
	d.nextCardIndex += numberOfCards
	return cards, nil
}

func (d *Deck) MustDrawN(numberOfCards int) []Card {
	cards, err := d.DrawN(numberOfCards)
	if err != nil {
		panic(err)
	}
	return cards
}

func (d *Deck) Exclude(cardsToRemove []Card) error {
	for _, card := range cardsToRemove {
		index := d.indexOf(card)
		if index == -1 {
			return newError(ErrCardNotInDeck, fmt.Sprintf("invariant violated: card to remove %s was not found in deck", card))
		}

		if index > d.nextCardIndex-1 {
			d.nextCardIndex++
			old := d.cards[index]
			copy(d.cards[1:index+1], d.cards[0:index])
			d.cards[0] = old
		}
	}

	return nil
}

func (d *Deck) indexOf(card Card) int {
	for i, deckCard := range d.cards {
		if deckCard.Equal(card) {
			return i
		}
	}
	return -1
}

func (d *Deck) Cards() []Card {
	return slices.Clone(d.cards)
}

func (d *Deck) Remaining() int {
	return len(d.cards) - d.nextCardIndex
}

func (d *Deck) Seed() int64 {
	return d.seed
}
