package pokeralgo

import "fmt"

func GetPrettyHandName(hand WinningHand) string {
	cardPrintLookUp := map[int]string{
		14: "Ace",
		13: "King",
		12: "Queen",
		11: "Jack",
		10: "10",
		9:  "9",
		8:  "8",
		7:  "7",
		6:  "6",
		5:  "5",
		4:  "4",
		3:  "3",
		2:  "2",
		1:  "Ace",
	}

	switch hand.Type {
	case RoyalFlush:
		return "Royal Flush"
	case StraightFlush:
		return fmt.Sprintf("%s-High Straight Flush", cardPrintLookUp[hand.Cards[4].Rank])
	case FourKind:
		return fmt.Sprintf("Four of a Kind, %ss", cardPrintLookUp[hand.Cards[4].Rank])
	case FullHouse:
		return fmt.Sprintf("Full House, %ss over %ss", cardPrintLookUp[hand.Cards[4].Rank], cardPrintLookUp[hand.Cards[0].Rank])
	case Flush:
		return fmt.Sprintf("%s-High Flush", cardPrintLookUp[hand.Cards[4].Rank])
	case Straight:
		return fmt.Sprintf("%s-High Straight", cardPrintLookUp[hand.Cards[4].Rank])
	case ThreeKind:
		return fmt.Sprintf("Three of a Kind, %ss", cardPrintLookUp[hand.Cards[4].Rank])
	case TwoPair:
		return fmt.Sprintf("Two Pair, %ss and %ss", cardPrintLookUp[hand.Cards[4].Rank], cardPrintLookUp[hand.Cards[2].Rank])
	case OnePair:
		return fmt.Sprintf("Pair of %ss", cardPrintLookUp[hand.Cards[4].Rank])
	case Nothing:
		return fmt.Sprintf("%s High Card", cardPrintLookUp[hand.Cards[4].Rank])
	default:
		return "Unknown Hand"
	}
}
