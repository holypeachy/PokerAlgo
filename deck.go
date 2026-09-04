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
	NextCardIndex int
	Seed          int64
}

func NewDeck() *Deck {
	seed := time.Now().UnixNano()
	return NewDeckWithSeed(seed)
}

func NewDeckWithSeed(seed int64) *Deck {
	deck := &Deck{
		cards: make([]Card, 0, 52),
		rand:  rand.New(rand.NewSource(seed)),
		Seed:  seed,
	}

	deck.create()
	deck.shuffle()
	return deck
}

func (d *Deck) create() {
	for _, suit := range []CardSuit{Spades, Clubs, Hearts, Diamonds} {
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

func (d *Deck) ResetDeck() int64 {
	d.Seed = time.Now().UnixNano()
	d.rand = rand.New(rand.NewSource(d.Seed))
	d.reset()
	return d.Seed
}

func (d *Deck) ResetDeckWithSeed(seed int64) {
	d.Seed = seed
	d.rand = rand.New(rand.NewSource(seed))
	d.reset()
}

func (d *Deck) reset() {
	d.cards = d.cards[:0]
	d.create()
	d.NextCardIndex = 0
	d.shuffle()
}

// Returns the first card, and then removes it from the deck
func (d *Deck) NextCard() (Card, error) {
	if d.NextCardIndex >= len(d.cards) {
		return Card{}, newError(ErrDeckEmpty, "no more cards in the deck")
	}

	card := d.cards[d.NextCardIndex]
	d.NextCardIndex++
	return card, nil
}

func (d *Deck) MustNextCard() Card {
	card, err := d.NextCard()
	if err != nil {
		panic(err)
	}
	return card
}

func (d *Deck) NextCards(numberOfCards int) ([]Card, error) {
	if numberOfCards < 1 {
		return nil, fmt.Errorf("numberOfCards must be greater than 0")
	}
	if d.NextCardIndex >= len(d.cards) {
		return nil, newError(ErrDeckEmpty, "no more cards in the deck")
	}
	if d.NextCardIndex+numberOfCards > len(d.cards) {
		return nil, newError(ErrNotEnoughCards, fmt.Sprintf("cards left: %d. Cards requested: %d", len(d.cards)-d.NextCardIndex, numberOfCards))
	}

	cards := slices.Clone(d.cards[d.NextCardIndex : d.NextCardIndex+numberOfCards])
	d.NextCardIndex += numberOfCards
	return cards, nil
}

func (d *Deck) MustNextCards(numberOfCards int) []Card {
	cards, err := d.NextCards(numberOfCards)
	if err != nil {
		panic(err)
	}
	return cards
}

func (d *Deck) RemoveCards(cardsToRemove []Card) error {
	for _, card := range cardsToRemove {
		index := d.indexOf(card)
		if index == -1 {
			return newError(ErrCardNotInDeck, fmt.Sprintf("invariant violated: card to remove %s was not found in deck", card))
		}

		if index > d.NextCardIndex-1 {
			d.NextCardIndex++
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

func (d *Deck) CopyOfCards() []Card {
	return slices.Clone(d.cards)
}
