package pokeralgo

import "fmt"

type HandType int

const (
	Nothing HandType = iota
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
	case Nothing:
		return "Nothing"
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

type Pair struct {
	First  Card
	Second Card
}

func (p Pair) String() string {
	return fmt.Sprintf("%s %s", p.First, p.Second)
}

type WinningHand struct {
	Type  HandType
	Cards []Card
}

func (h WinningHand) String() string {
	return fmt.Sprintf("WinningHand: %d - Cards: %v", h.Type, h.Cards)
}

type Player struct {
	Name        string
	HoleCards   Pair
	WinningHand *WinningHand
}

func NewPlayer(name string, first Card, second Card) Player {
	first.IsPlayerCard = true
	second.IsPlayerCard = true

	return Player{
		Name:        name,
		HoleCards:   Pair{First: first, Second: second},
		WinningHand: nil,
	}
}

func (p *Player) NewHand(first Card, second Card) {
	first.IsPlayerCard = true
	second.IsPlayerCard = true
	p.HoleCards = Pair{First: first, Second: second}
	p.WinningHand = nil
}

func (p Player) String() string {
	return fmt.Sprintf("%s: %s %s", p.Name, p.HoleCards.First, p.HoleCards.Second)
}
