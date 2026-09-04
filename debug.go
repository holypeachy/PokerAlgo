package pokeralgo

import (
	"fmt"
	"io"
	"os"
	"strings"
)

type debugLevel int

const (
	debugOff debugLevel = iota
	debugProgress
	debugEverything
)

var debugVerbosity = debugOff
var debugOutput io.Writer = os.Stdout

func debugLog(log string, verbosity debugLevel) {
	if debugVerbosity >= verbosity {
		fmt.Fprintln(debugOutput, log)
	}
}

func debugLogCards(description string, cards []Card) {
	if debugVerbosity >= debugEverything {
		fmt.Fprintf(debugOutput, "🤖 %s: \n", description)
		if len(cards) > 0 {
			fmt.Fprintf(debugOutput, "\t%s\n", cardsToString(cards))
		}
		fmt.Fprintln(debugOutput)
	}
}

func debugLogPlayers(description string, players []Player) {
	if debugVerbosity >= debugProgress {
		fmt.Fprintf(debugOutput, "🤖 %s:\n", description)
		for _, player := range players {
			handName := "<nil>"
			cards := ""
			if player.WinningHand != nil {
				handName = GetPrettyHandName(*player.WinningHand)
				cards = cardsToString(player.WinningHand.Cards)
			}
			fmt.Fprintf(debugOutput, "\t %s  %s  %s \n\n", player.Name, handName, cards)
		}
	}
}

func debugLogWinners(winners []Player) {
	if debugVerbosity >= debugProgress {
		fmt.Fprintln(debugOutput, "🥇 Winner(s):")
		for _, player := range winners {
			handName := "<nil>"
			cards := ""
			if player.WinningHand != nil {
				handName = GetPrettyHandName(*player.WinningHand)
				cards = cardsToString(player.WinningHand.Cards)
			}
			fmt.Fprintf(debugOutput, "\t %s  %s  %s \n", player.Name, handName, cards)
		}
	}
}

func debugLogDeterminingHand(playerName string) {
	if debugVerbosity >= debugProgress {
		fmt.Fprintln(debugOutput)
		fmt.Fprintf(debugOutput, "💭 Determining Hand for '%s'\n", playerName)
	}
}

func debugLogWinningHand(handType HandType, cards []Card) {
	if debugVerbosity >= debugProgress {
		fmt.Fprintf(debugOutput, " %s:  %s \n", handType, cardsToString(cards))
	}
}

func cardsToString(cards []Card) string {
	parts := make([]string, 0, len(cards))
	for _, card := range cards {
		parts = append(parts, card.String())
	}
	return strings.Join(parts, " ")
}
