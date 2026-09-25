package pokeralgo

import (
	"slices"
	"sort"
)

func DetermineWinners(players []Player, communityCards []Card) ([]Player, error) {
	if err := argsGetWinners(players, communityCards); err != nil {
		return nil, err
	}

	debugf(debugSummary, "🔎 [algo] evaluating %d players", len(players))
	playersWithHands := slices.Clone(players)
	for i := range playersWithHands {
		debugEvaluationStart(playersWithHands[i].Name)

		combinedCards := []Card{
			playersWithHands[i].HoleCards.First,
			playersWithHands[i].HoleCards.Second,
		}
		combinedCards = append(combinedCards, communityCards...)

		winningHand, err := Evaluate(combinedCards)
		if err != nil {
			return nil, err
		}
		playersWithHands[i].BestHand = &winningHand
	}

	debugf(debugSummary, "\n🔎 [algo] comparing evaluated hands")
	return determineWinners(playersWithHands)
}

func determineWinners(allPlayers []Player) ([]Player, error) {
	// Order from highest to lowest hand value
	players := slices.Clone(allPlayers)
	sortPlayersByWinningHandTypeDesc(players)

	debugPlayers(debugSummary, "players after sorting", players)

	winners, err := breakTies(players)
	if err != nil {
		return nil, err
	}
	if len(winners) < 1 {
		return nil, newError(ErrInternalPokerAlgo, "invariant violated: winners count should never be less than 1")
	}
	debugWinners(winners)

	return winners, nil
}

func sortPlayersByWinningHandTypeDesc(players []Player) {
	sort.SliceStable(players, func(i, j int) bool {
		return players[i].BestHand.Type > players[j].BestHand.Type
	})
}

func breakTies(players []Player) ([]Player, error) {
	winners := slices.Clone(players)
	tempPlayers := slices.Clone(winners)

	hasChangesBeenMade := true
	for hasChangesBeenMade && len(winners) > 1 {
		hasChangesBeenMade = false
		for playerIndex := 0; playerIndex < len(winners)-1; playerIndex++ {
			result, err := compareWinningHands(winners[playerIndex].BestHand, winners[playerIndex+1].BestHand)
			if err != nil {
				return nil, err
			}

			message := "tie"
			if result == -1 {
				message = winners[playerIndex].Name + " wins"
			} else if result == 1 {
				message = winners[playerIndex+1].Name + " wins"
			}
			debugf(
				debugTrace,
				"⚖️ [compare] %s vs %s: %s",
				winners[playerIndex].Name,
				winners[playerIndex+1].Name,
				message,
			)

			if result == -1 {
				if winners[playerIndex].BestHand.Type > winners[playerIndex+1].BestHand.Type {
					debugf(debugTrace, "⚖️ [compare] different hand types: remaining players cannot win")

					for k := playerIndex + 1; k < len(winners); k++ {
						tempPlayers = removePlayer(tempPlayers, winners[k])
					}
				} else {
					tempPlayers = removePlayer(tempPlayers, winners[playerIndex+1])
				}
				hasChangesBeenMade = true
				break
			} else if result == 1 {
				tempPlayers = removePlayer(tempPlayers, winners[playerIndex])
				hasChangesBeenMade = true
			} else if result != 0 {
				return nil, newError(ErrInternalPokerAlgo, "invariant violated: compareWinningHands returned unexpected result")
			}
		}
		winners = slices.Clone(tempPlayers)
	}

	return winners, nil
}

// -1 left wins, 0 tie, 1 right wins
func compareWinningHands(left *Hand, right *Hand) (int, error) {
	if left == nil || right == nil {
		return 0, newError(ErrInternalPokerAlgo, "invariant violated: a passed winning hand argument is nil")
	}

	debugCards(debugTrace, "compare", "left hand", left.Cards)
	debugCards(debugTrace, "compare", "right hand", right.Cards)

	if left.Type > right.Type {
		return -1, nil
	}
	if right.Type > left.Type {
		return 1, nil
	}

	leftCards := left.Cards
	rightCards := right.Cards

	switch left.Type {
	case RoyalFlush:
		return 0, nil
	case StraightFlush:
		return compareKickers(leftCards, rightCards)
	case FourKind:
		if leftCards[4].Rank > rightCards[4].Rank {
			return -1, nil
		} else if rightCards[4].Rank > leftCards[4].Rank {
			return 1, nil
		}
		return compareKickers(leftCards[0:1], rightCards[0:1])
	case FullHouse:
		if leftCards[4].Rank > rightCards[4].Rank {
			return -1, nil
		} else if rightCards[4].Rank > leftCards[4].Rank {
			return 1, nil
		} else if leftCards[0].Rank > rightCards[0].Rank {
			return -1, nil
		} else if rightCards[0].Rank > leftCards[0].Rank {
			return 1, nil
		}
		return 0, nil
	case Flush:
		return compareKickers(leftCards, rightCards)
	case Straight:
		return compareKickers(leftCards, rightCards)
	case ThreeKind:
		if leftCards[4].Rank > rightCards[4].Rank {
			return -1, nil
		} else if rightCards[4].Rank > leftCards[4].Rank {
			return 1, nil
		}
		return compareKickers(leftCards[0:2], rightCards[0:2])
	case TwoPair:
		if leftCards[4].Rank > rightCards[4].Rank {
			return -1, nil
		} else if rightCards[4].Rank > leftCards[4].Rank {
			return 1, nil
		}
		if leftCards[2].Rank > rightCards[2].Rank {
			return -1, nil
		} else if rightCards[2].Rank > leftCards[2].Rank {
			return 1, nil
		}
		return compareKickers(leftCards[0:1], rightCards[0:1])
	case OnePair:
		if leftCards[4].Rank > rightCards[4].Rank {
			return -1, nil
		} else if rightCards[4].Rank > leftCards[4].Rank {
			return 1, nil
		}
		return compareKickers(leftCards[0:3], rightCards[0:3])
	case HighCard:
		return compareKickers(leftCards, rightCards)
	default:
		return 0, newError(ErrInternalPokerAlgo, "invariant violated: switch defaulted")
	}
}

// -1 left wins, 0 tie, 1 right wins
func compareKickers(left []Card, right []Card) (int, error) {
	debugCards(debugTrace, "compare", "left kickers", left)
	debugCards(debugTrace, "compare", "right kickers", right)

	if len(left) != len(right) {
		return 0, newError(ErrInternalPokerAlgo, "invariant violated: left and right kicker counts differ")
	}

	for i := len(left) - 1; i >= 0; i-- {
		if left[i].Rank > right[i].Rank {
			debugf(debugTrace, "⚖️ [compare] kicker result: left wins")
			return -1, nil
		} else if right[i].Rank > left[i].Rank {
			debugf(debugTrace, "⚖️ [compare] kicker result: right wins")
			return 1, nil
		}
	}

	debugf(debugTrace, "⚖️ [compare] kicker result: tie")

	return 0, nil
}

func removePlayer(players []Player, target Player) []Player {
	for i, player := range players {
		if samePlayer(player, target) {
			return append(players[:i], players[i+1:]...)
		}
	}
	return players
}

func samePlayer(left Player, right Player) bool {
	return left.Name == right.Name && left.HoleCards.First.Equal(right.HoleCards.First) && left.HoleCards.Second.Equal(right.HoleCards.Second)
}
