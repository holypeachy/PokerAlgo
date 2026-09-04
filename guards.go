package pokeralgo

import (
	"fmt"
	"slices"
)

func argsGetWinners(players []Player, communityCards []Card) error {
	if len(players) < 2 {
		return fmt.Errorf("there must be at least 2 players")
	}
	if len(communityCards) != 5 {
		return fmt.Errorf("for the showdown, there must be all 5 community cards")
	}

	allCards := slices.Clone(communityCards)
	for _, player := range players {
		allCards = append(allCards, player.HoleCards.First, player.HoleCards.Second)
	}

	return validateUniqueAndNoLowAces(allCards, "either players or communityCards arguments have duplicate cards")
}

func argsGetWinningHand(cards []Card) error {
	if len(cards) < 5 || len(cards) > 7 {
		return fmt.Errorf("the list must have 5-7 cards")
	}

	return validateUniqueAndNoLowAces(cards, "cards argument has duplicate cards")
}

func argsWinningChanceSim(playerHoleCards Pair, communityCards []Card, numOfOpponents int, numberOfSimulatedGames int) error {
	if len(communityCards) < 3 {
		return fmt.Errorf("there should be no less than 3 community cards")
	}
	if len(communityCards) > 5 {
		return fmt.Errorf("there should be no more than 5 community cards")
	}
	if numOfOpponents < 1 {
		return fmt.Errorf("there should be at least 1 opponent")
	}
	if numberOfSimulatedGames < 100 {
		return fmt.Errorf("number of simulated games is less than 100")
	}

	allCards := slices.Clone(communityCards)
	allCards = append(allCards, playerHoleCards.First, playerHoleCards.Second)
	return validateUnique(allCards, "either playerHoleCards or communityCards arguments have duplicate cards")
}

func argsPreFlopSim(playerHoleCards Pair, numOfOpponents int, numberOfSimulatedGames int) error {
	if numOfOpponents < 1 {
		return fmt.Errorf("there should be at least 1 opponent")
	}
	if numberOfSimulatedGames < 100 {
		return fmt.Errorf("number of simulated games is less than 100")
	}
	return againstDuplicateHoleCards(playerHoleCards, "playerHoleCards")
}

func argsPreFlopLookUp(playerHoleCards Pair, numOfOpponents int) error {
	if numOfOpponents < 1 {
		return fmt.Errorf("there should be at least 1 opponent")
	}
	return againstDuplicateHoleCards(playerHoleCards, "playerHoleCards")
}

func againstDuplicateHoleCards(playerHoleCards Pair, paramName string) error {
	if playerHoleCards.First.Equal(playerHoleCards.Second) {
		return newError(ErrDuplicateCards, fmt.Sprintf("%s argument has duplicate cards", paramName))
	}
	return nil
}

func validateUniqueAndNoLowAces(cards []Card, duplicateMessage string) error {
	if err := validateUnique(cards, duplicateMessage); err != nil {
		return err
	}

	for _, card := range cards {
		if card.Rank == 1 {
			return newError(ErrLowAces, "when instantiating Ace cards use rank 14 not 1")
		}
	}

	return nil
}

func validateUnique(cards []Card, duplicateMessage string) error {
	seen := make(map[[2]int]struct{}, len(cards))
	for _, card := range cards {
		key := [2]int{card.Rank, int(card.Suit)}
		if _, ok := seen[key]; ok {
			return newError(ErrDuplicateCards, duplicateMessage)
		}
		seen[key] = struct{}{}
	}
	return nil
}
