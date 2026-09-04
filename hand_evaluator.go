package pokeralgo

import (
	"slices"
	"sort"
)

func GetWinningHand(combinedCards []Card) (WinningHand, error) {
	if err := argsGetWinningHand(combinedCards); err != nil {
		return WinningHand{}, err
	}

	cardsCopy := slices.Clone(combinedCards)
	sortCardsByValue(cardsCopy)

	debugLogCards("HandEvaluator.GetWinningHand() - All Cards", cardsCopy)

	return evaluateHand(cardsCopy)
}

func GetWinningHandForPlayer(playerHoleCards Pair, communityCards []Card) (WinningHand, error) {
	cards := []Card{playerHoleCards.First, playerHoleCards.Second}
	cards = append(cards, communityCards...)
	return GetWinningHand(cards)
}

func evaluateHand(cards []Card) (WinningHand, error) {
	flushCards := cardsBySuitCount(cards, 5)
	fourKind := cardsByRankCount(cards, 4)
	threeKinds := cardsByRankCount(cards, 3)
	sortCardsByValue(threeKinds)
	pairs := cardsByRankCount(cards, 2)
	sortCardsByValue(pairs)

	debugLogCards("HandEvaluator.EvaluateHand() - Flush Cards", flushCards)
	debugLogCards("HandEvaluator.EvaluateHand() - Four Kind Cards", fourKind)
	debugLogCards("HandEvaluator.EvaluateHand() - Three Kind Cards", threeKinds)
	debugLogCards("HandEvaluator.EvaluateHand() - Pair Cards", pairs)

	var bestFive []Card

	// ! Royal Flush
	if len(flushCards) >= 5 {
		bestFive = getBestFiveCards(flushCards)
		if bestFive[0].Rank == 10 && hasConsecutiveValues(bestFive) {
			return newWinningHand(RoyalFlush, bestFive), nil
		}

		// ! Straight Flush
		flushCardsWithLowAces := addLowAces(flushCards)
		for i := len(flushCardsWithLowAces) - 5; i >= 0; i-- {
			bestFive = slices.Clone(flushCardsWithLowAces[i : i+5])
			if hasConsecutiveValues(bestFive) {
				return newWinningHand(StraightFlush, bestFive), nil
			}
		}
		flushCards = removeLowAces(flushCardsWithLowAces)
	}

	// ! Four Kind
	if len(fourKind) == 4 {
		completeHand, err := completeWinningHand(fourKind, cards)
		if err != nil {
			return WinningHand{}, err
		}
		return newWinningHand(FourKind, completeHand), nil
	}

	// ! Two, 3 of a kind. Full House
	if len(threeKinds) == 6 {
		top3 := slices.Clone(threeKinds[3:6])
		bottom3 := slices.Clone(threeKinds[0:3])
		fullHouse := make([]Card, 0, 5)

		if bottom3[0].IsPlayerCard && bottom3[1].IsPlayerCard {
			fullHouse = append(fullHouse, bottom3[0:2]...)
		} else if bottom3[1].IsPlayerCard && bottom3[2].IsPlayerCard {
			fullHouse = append(fullHouse, bottom3[1:3]...)
		} else if bottom3[0].IsPlayerCard && bottom3[2].IsPlayerCard {
			fullHouse = append(fullHouse, bottom3[0], bottom3[2])
		} else {
			fullHouse = append(fullHouse, bottom3[1:3]...)
		}

		fullHouse = append(fullHouse, top3...)
		return newWinningHand(FullHouse, fullHouse), nil
	} else if len(threeKinds) == 3 && len(pairs) >= 2 {
		// ! One, 3 of a kind and 1 or 2 pairs. Full House
		topPair := slices.Clone(pairs[len(pairs)-2:])
		fullHouse := slices.Clone(topPair)
		fullHouse = append(fullHouse, threeKinds...)
		return newWinningHand(FullHouse, fullHouse), nil
	}

	// ! Standard Flush
	if len(flushCards) >= 5 {
		bestFive = slices.Clone(flushCards[len(flushCards)-5:])
		return newWinningHand(Flush, bestFive), nil
	}

	// ! Straight
	tempCards := slices.Clone(cards)
	tempCards = addLowAces(tempCards)

	// Removes duplicates
	for i := len(tempCards) - 1; i > 0; i-- {
		if tempCards[i].Rank == tempCards[i-1].Rank {
			if (tempCards[i].IsPlayerCard && tempCards[i-1].IsPlayerCard) || (!tempCards[i].IsPlayerCard && !tempCards[i-1].IsPlayerCard) {
				tempCards = removeAt(tempCards, i)
			} else if tempCards[i].IsPlayerCard {
				tempCards = removeAt(tempCards, i-1)
			} else {
				tempCards = removeAt(tempCards, i)
			}
		}
	}

	debugLogCards("HandEvaluator.EvaluateHand() - Without Duplicates", tempCards)

	for i := len(tempCards) - 5; i >= 0; i-- {
		bestFive = slices.Clone(tempCards[i : i+5])
		if hasConsecutiveValues(bestFive) {
			return newWinningHand(Straight, bestFive), nil
		}
	}

	// ! Three of a kind
	if len(threeKinds) == 3 {
		completeHand, err := completeWinningHand(threeKinds, cards)
		if err != nil {
			return WinningHand{}, err
		}
		return newWinningHand(ThreeKind, completeHand), nil
	} else if len(pairs) >= 4 {
		// ! 2 Pairs or more
		topPair := slices.Clone(pairs[len(pairs)-2:])
		bottomPair := slices.Clone(pairs[len(pairs)-4 : len(pairs)-2])

		twoPairs := slices.Clone(bottomPair)
		twoPairs = append(twoPairs, topPair...)
		completeHand, err := completeWinningHand(twoPairs, cards)
		if err != nil {
			return WinningHand{}, err
		}
		return newWinningHand(TwoPair, completeHand), nil
	} else if len(pairs) == 2 {
		// ! 1 Pair
		completeHand, err := completeWinningHand(pairs, cards)
		if err != nil {
			return WinningHand{}, err
		}
		return newWinningHand(OnePair, completeHand), nil
	}

	completeHand, err := completeWinningHand(nil, cards)
	if err != nil {
		return WinningHand{}, err
	}
	return newWinningHand(Nothing, completeHand), nil
}

func cardsBySuitCount(cards []Card, count int) []Card {
	groupCounts := make(map[CardSuit]int)
	for _, card := range cards {
		groupCounts[card.Suit]++
	}

	result := make([]Card, 0)
	for _, card := range cards {
		if groupCounts[card.Suit] >= count {
			result = append(result, card)
		}
	}
	return result
}

func cardsByRankCount(cards []Card, count int) []Card {
	groupCounts := make(map[int]int)
	for _, card := range cards {
		groupCounts[card.Rank]++
	}

	result := make([]Card, 0)
	for _, card := range cards {
		if groupCounts[card.Rank] == count {
			result = append(result, card)
		}
	}
	return result
}

func completeWinningHand(winningCards []Card, allCards []Card) ([]Card, error) {
	completeHand := slices.Clone(winningCards)
	neededNumberOfCards := 5 - len(winningCards)
	remainingCards := exceptCards(allCards, winningCards)
	debugLogCards("HandEvaluator.CompleteWinningHand() - remainingCards", remainingCards)

	if neededNumberOfCards < 1 {
		return nil, newError(ErrInternalPokerAlgo, "invariant violation: neededNumberOfCards is less than 1")
	}

	for neededNumberOfCards > 0 {
		lastIndex := len(remainingCards) - 1
		completeHand = append([]Card{remainingCards[lastIndex]}, completeHand...)
		remainingCards = remainingCards[:lastIndex]
		neededNumberOfCards--
	}

	if len(completeHand) != 5 {
		return nil, newError(ErrInternalPokerAlgo, "invariant violation: completeHand count must be 5")
	}

	return completeHand, nil
}

func newWinningHand(handType HandType, cards []Card) WinningHand {
	debugLogWinningHand(handType, cards)
	return WinningHand{Type: handType, Cards: cards}
}

func getBestFiveCards(cards []Card) []Card {
	return slices.Clone(cards[len(cards)-5:])
}

func addLowAces(cards []Card) []Card {
	acesToAdd := make([]Card, 0)
	for _, card := range cards {
		if card.Rank == 14 {
			acesToAdd = append(acesToAdd, Card{Rank: 1, Suit: card.Suit, IsPlayerCard: card.IsPlayerCard})
		}
	}

	result := slices.Clone(acesToAdd)
	result = append(result, cards...)
	return result
}

func removeLowAces(cards []Card) []Card {
	result := make([]Card, 0, len(cards))
	for _, card := range cards {
		if card.Rank != 1 {
			result = append(result, card)
		}
	}
	return result
}

func sortCardsByValue(cards []Card) {
	sort.SliceStable(cards, func(i, j int) bool {
		return cards[i].Rank < cards[j].Rank
	})
}

func hasConsecutiveValues(cards []Card) bool {
	startingValue := 0
	for i, card := range cards {
		if i == 0 {
			startingValue = card.Rank
			continue
		}
		startingValue++
		if card.Rank != startingValue {
			return false
		}
	}
	return true
}

func exceptCards(allCards []Card, cardsToExclude []Card) []Card {
	result := make([]Card, 0, len(allCards))
	for _, card := range allCards {
		found := false
		for _, excluded := range cardsToExclude {
			if card.Equal(excluded) {
				found = true
				break
			}
		}
		if !found {
			result = append(result, card)
		}
	}
	return result
}

func removeAt(cards []Card, index int) []Card {
	return append(cards[:index], cards[index+1:]...)
}
