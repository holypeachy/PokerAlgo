package pokeralgo

import (
	"fmt"
	"log"
	"os"
	"strings"
)

type debugLevel int

const (
	debugOff debugLevel = iota
	debugSummary
	debugTrace
)

var currentDebugLevel = debugOff
var debugLogger = log.New(os.Stderr, "", 0)

func SetDebugLevel(level string) error {
	switch level {
	case "off":
		currentDebugLevel = debugOff
	case "summary":
		currentDebugLevel = debugSummary
	case "trace":
		currentDebugLevel = debugTrace
	default:
		return newError(ErrInvalidArgument, "debug level must be off, summary, or trace")
	}

	return nil
}

func debugf(level debugLevel, format string, args ...any) {
	if currentDebugLevel >= level {
		debugLogger.Printf(format, args...)
	}
}

func debugCards(level debugLevel, component string, description string, cards []Card) {
	if currentDebugLevel < level {
		return
	}

	formattedCards := "<none>"
	if len(cards) > 0 {
		formattedCards = formatCards(cards)
	}

	debugLogger.Printf("   [%s] %s\n      %s", component, description, formattedCards)
}

func debugPlayers(level debugLevel, description string, players []Player) {
	if currentDebugLevel < level {
		return
	}

	var output strings.Builder
	fmt.Fprintf(&output, "👥 [algo] %s", description)
	for _, player := range players {
		handName := "<nil>"
		cards := ""
		if player.BestHand != nil {
			handName = DescribeHand(*player.BestHand)
			cards = formatCards(player.BestHand.Cards)
		}
		fmt.Fprintf(&output, "\n   %s | %s | %s", player.Name, handName, cards)
	}

	debugLogger.Print(output.String())
}

func debugWinners(winners []Player) {
	if currentDebugLevel < debugSummary {
		return
	}

	title := "winner"
	if len(winners) != 1 {
		title = "winners"
	}

	var output strings.Builder
	fmt.Fprintf(&output, "🥇 [algo] %s", title)
	for _, player := range winners {
		handName := "<nil>"
		cards := ""
		if player.BestHand != nil {
			handName = DescribeHand(*player.BestHand)
			cards = formatCards(player.BestHand.Cards)
		}
		fmt.Fprintf(&output, "\n   %s | %s | %s", player.Name, handName, cards)
	}

	debugLogger.Print(output.String())
}

func debugEvaluationStart(playerName string) {
	if currentDebugLevel >= debugSummary {
		debugLogger.Printf("\n💭 [evaluate] player %q", playerName)
	}
}

func debugEvaluationResult(handType HandType, cards []Card) {
	if currentDebugLevel >= debugSummary {
		handName := DescribeHand(Hand{Type: handType, Cards: cards})
		debugLogger.Printf("✅ [evaluate] %s\n   %s", handName, formatCards(cards))
	}
}

func formatCards(cards []Card) string {
	parts := make([]string, 0, len(cards))
	for _, card := range cards {
		parts = append(parts, card.String())
	}
	return strings.Join(parts, " ")
}
