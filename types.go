package pokeralgo

import "fmt"

type HandType int

const (
	HighCard HandType = iota
	OnePair
	TwoPair
	ThreeKind
	Straight
	Flush
	FullHouse
	FourKind
	StraightFlush
	RoyalFlush
)

func (h HandType) String() string {
	switch h {
	case HighCard:
		return "HighCard"
	case OnePair:
		return "OnePair"
	case TwoPair:
		return "TwoPair"
	case ThreeKind:
		return "ThreeKind"
	case Straight:
		return "Straight"
	case Flush:
		return "Flush"
	case FullHouse:
		return "FullHouse"
	case FourKind:
		return "FourKind"
	case StraightFlush:
		return "StraightFlush"
	case RoyalFlush:
		return "RoyalFlush"
	default:
		return fmt.Sprintf("HandType(%d)", h)
	}
}

type HoleCards struct {
	First  Card
	Second Card
}

func (p HoleCards) String() string {
	return fmt.Sprintf("%s %s", p.First, p.Second)
}

type Hand struct {
	Type  HandType
	Cards []Card
}

func (h Hand) String() string {
	return fmt.Sprintf("WinningHand: %d - Cards: %v", h.Type, h.Cards)
}

type Player struct {
	Name      string
	HoleCards HoleCards
	BestHand  *Hand
}

func NewPlayer(name string, first Card, second Card) Player {
	first.IsHoleCard = true
	second.IsHoleCard = true

	return Player{
		Name:      name,
		HoleCards: HoleCards{First: first, Second: second},
		BestHand:  nil,
	}
}

func (p *Player) SetHoleCards(first Card, second Card) {
	first.IsHoleCard = true
	second.IsHoleCard = true
	p.HoleCards = HoleCards{First: first, Second: second}
	p.BestHand = nil
}

func (p Player) String() string {
	return fmt.Sprintf("%s: %s %s", p.Name, p.HoleCards.First, p.HoleCards.Second)
}
