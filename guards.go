package pokeralgo

import (
	"fmt"
	"slices"
)

func validatePlayersAndBoard(players []Player, communityCards []Card) error {
	if len(players) < 2 {
		return fmt.Errorf("%w: there must be at least 2 players", ErrInvalidArgument)
	}
	if len(communityCards) != 5 {
		return fmt.Errorf("%w: for the showdown, there must be all 5 community cards", ErrInvalidArgument)
	}

	allCards := slices.Clone(communityCards)
	for _, player := range players {
		allCards = append(allCards, player.HoleCards.First, player.HoleCards.Second)
	}

	return validateUniqueCardsAndNoLowAces(allCards, "either players or communityCards arguments have duplicate cards")
}

func validateEvaluationCards(cards []Card) error {
	if len(cards) < 5 || len(cards) > 7 {
		return fmt.Errorf("%w: the list must have 5-7 cards", ErrInvalidArgument)
	}

	return validateUniqueCardsAndNoLowAces(cards, "cards argument has duplicate cards")
}

func validateSimulation(playerHoleCards HoleCards, communityCards []Card, numOfOpponents int, numberOfSimulatedGames int) error {
	if len(communityCards) < 3 {
		return fmt.Errorf("%w: there should be no less than 3 community cards", ErrInvalidArgument)
	}
	if len(communityCards) > 5 {
		return fmt.Errorf("%w: there should be no more than 5 community cards", ErrInvalidArgument)
	}
	if numOfOpponents < 1 {
		return fmt.Errorf("%w: there should be at least 1 opponent", ErrInvalidArgument)
	}
	if numOfOpponents > 5 {
		return fmt.Errorf("%w: there should be no more than 5 opponents", ErrInvalidArgument)
	}
	if numberOfSimulatedGames < 100 {
		return fmt.Errorf("%w: number of simulated games is less than 100", ErrInvalidArgument)
	}

	allCards := slices.Clone(communityCards)
	allCards = append(allCards, playerHoleCards.First, playerHoleCards.Second)
	return validateUniqueCards(allCards, "either playerHoleCards or communityCards arguments have duplicate cards")
}

func validatePreflopSimulation(playerHoleCards HoleCards, numOfOpponents int, numberOfSimulatedGames int) error {
	if numOfOpponents < 1 {
		return fmt.Errorf("%w: there should be at least 1 opponent", ErrInvalidArgument)
	}
	if numOfOpponents > 5 {
		return fmt.Errorf("%w: there should be no more than 5 opponents", ErrInvalidArgument)
	}
	if numberOfSimulatedGames < 100 {
		return fmt.Errorf("%w: number of simulated games is less than 100", ErrInvalidArgument)
	}
	return validateHoleCards(playerHoleCards, "playerHoleCards")
}

func validatePreflopLookup(playerHoleCards HoleCards, numOfOpponents int) error {
	if numOfOpponents < 1 {
		return fmt.Errorf("%w: there should be at least 1 opponent", ErrInvalidArgument)
	}
	return validateHoleCards(playerHoleCards, "playerHoleCards")
}

func validateHoleCards(playerHoleCards HoleCards, paramName string) error {
	if playerHoleCards.First.Equal(playerHoleCards.Second) {
		return fmt.Errorf("%w: %s argument has duplicate cards", ErrDuplicateCards, paramName)
	}
	return nil
}

func validateUniqueCardsAndNoLowAces(cards []Card, duplicateMessage string) error {
	if err := validateUniqueCards(cards, duplicateMessage); err != nil {
		return err
	}

	for _, card := range cards {
		if card.Rank == 1 {
			return fmt.Errorf("%w: when instantiating Ace cards use rank 14 not 1", ErrLowAces)
		}
	}

	return nil
}

func validateUniqueCards(cards []Card, duplicateMessage string) error {
	seen := make(map[[2]int]struct{}, len(cards))
	for _, card := range cards {
		key := [2]int{card.Rank, int(card.Suit)}
		if _, ok := seen[key]; ok {
			return fmt.Errorf("%w: %s", ErrDuplicateCards, duplicateMessage)
		}
		seen[key] = struct{}{}
	}
	return nil
}
