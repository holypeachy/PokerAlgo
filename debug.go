package pokeralgo

import (
	"fmt"
	"log"
	"os"
	"strings"
)

type DebugLevel int

const (
	DebugOff DebugLevel = iota
	DebugSummary
	DebugTrace
)

var currentDebugLevel = DebugOff
var debugLogger = log.New(os.Stderr, "", 0)

func SetDebugLevel(lvl DebugLevel) {
	currentDebugLevel = lvl
}

func debugf(level DebugLevel, format string, args ...any) {
	if currentDebugLevel >= level {
		debugLogger.Printf(format, args...)
	}
}

func debugCards(level DebugLevel, component string, description string, cards []Card) {
	if currentDebugLevel < level {
		return
	}

	formattedCards := "<none>"
	if len(cards) > 0 {
		formattedCards = formatCards(cards)
	}

	debugLogger.Printf("   [%s] %s\n      %s", component, description, formattedCards)
}

func debugPlayers(level DebugLevel, description string, players []Player) {
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
	if currentDebugLevel < DebugSummary {
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
	if currentDebugLevel >= DebugSummary {
		debugLogger.Printf("\n💭 [evaluate] player %q", playerName)
	}
}

func debugEvaluationResult(handType HandType, cards []Card) {
	if currentDebugLevel >= DebugSummary {
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
