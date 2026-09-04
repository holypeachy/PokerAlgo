package pokeralgo

import (
	"fmt"
	"math"
	"runtime"
	"slices"
	"sync"
)

const (
	handStrengthSensitivity = 0.175 // Logistic Growth Rate of sigmoid
	baselineWinRate         = -1.85 // Logistic Shift of sigmoid
)

var compactCardPrintLookUp = map[int]string{
	1: "A", 2: "2", 3: "3", 4: "4", 5: "5", 6: "6", 7: "7",
	8: "8", 9: "9", 10: "T", 11: "J", 12: "Q", 13: "K", 14: "A",
}

// Returns Win and Tie Values from 0 to 1.0
func GetWinningChanceSimParallel(playerHoleCards Pair, communityCards []Card, numOfOpponents int, numberOfSimulatedGames int) (Chance, error) {
	if err := argsWinningChanceSim(playerHoleCards, communityCards, numOfOpponents, numberOfSimulatedGames); err != nil {
		return Chance{}, err
	}

	numTasks := runtime.NumCPU()
	simPerTask := numberOfSimulatedGames / numTasks
	remainder := numberOfSimulatedGames % numTasks

	type result struct {
		wins int
		ties int
		err  error
	}

	results := make(chan result, numTasks)
	var wg sync.WaitGroup
	for taskIndex := 0; taskIndex < numTasks; taskIndex++ {
		simsThisTask := simPerTask
		if taskIndex == 0 {
			simsThisTask += remainder // put remainder in first task
		}

		wg.Add(1)
		go func() {
			defer wg.Done()
			wins, ties, err := winningChanceSimTask(playerHoleCards, communityCards, numOfOpponents, simsThisTask)
			results <- result{wins: wins, ties: ties, err: err}
		}()
	}

	wg.Wait()
	close(results)

	totalWins := 0
	totalTies := 0
	for taskResult := range results {
		if taskResult.err != nil {
			return Chance{}, taskResult.err
		}
		totalWins += taskResult.wins
		totalTies += taskResult.ties
	}

	return Chance{WinChance: float64(totalWins) / float64(numberOfSimulatedGames), TieChance: float64(totalTies) / float64(numberOfSimulatedGames)}, nil
}

// Returns Win and Tie Values from 0 to 1.0
func GetWinningChancePreFlopSimParallel(playerHoleCards Pair, numOfOpponents int, numberOfSimulatedGames int) (Chance, error) {
	if err := argsPreFlopSim(playerHoleCards, numOfOpponents, numberOfSimulatedGames); err != nil {
		return Chance{}, err
	}

	numTasks := runtime.NumCPU()
	simPerTask := numberOfSimulatedGames / numTasks
	remainder := numberOfSimulatedGames % numTasks

	type result struct {
		wins int
		ties int
		err  error
	}

	results := make(chan result, numTasks)
	var wg sync.WaitGroup
	for taskIndex := 0; taskIndex < numTasks; taskIndex++ {
		simsThisTask := simPerTask
		if taskIndex == 0 {
			simsThisTask += remainder // put remainder in first task
		}

		wg.Add(1)
		go func() {
			defer wg.Done()
			wins, ties, err := winningChancePreFlopSimTask(playerHoleCards, numOfOpponents, simsThisTask)
			results <- result{wins: wins, ties: ties, err: err}
		}()
	}

	wg.Wait()
	close(results)

	totalWins := 0
	totalTies := 0
	for taskResult := range results {
		if taskResult.err != nil {
			return Chance{}, taskResult.err
		}
		totalWins += taskResult.wins
		totalTies += taskResult.ties
	}

	return Chance{WinChance: float64(totalWins) / float64(numberOfSimulatedGames), TieChance: float64(totalTies) / float64(numberOfSimulatedGames)}, nil
}

// Returns Win and Tie Values from 0 to 1.0
func GetWinningChanceSim(playerHoleCards Pair, communityCards []Card, numOfOpponents int, numberOfSimulatedGames int) (Chance, error) {
	if err := argsWinningChanceSim(playerHoleCards, communityCards, numOfOpponents, numberOfSimulatedGames); err != nil {
		return Chance{}, err
	}

	wins, ties, err := winningChanceSimTask(playerHoleCards, communityCards, numOfOpponents, numberOfSimulatedGames)
	if err != nil {
		return Chance{}, err
	}

	return Chance{WinChance: float64(wins) / float64(numberOfSimulatedGames), TieChance: float64(ties) / float64(numberOfSimulatedGames)}, nil
}

// Returns Win and Tie Values from 0 to 1.0
func GetWinningChancePreFlopSim(playerHoleCards Pair, numOfOpponents int, numberOfSimulatedGames int) (Chance, error) {
	if err := argsPreFlopSim(playerHoleCards, numOfOpponents, numberOfSimulatedGames); err != nil {
		return Chance{}, err
	}

	wins, ties, err := winningChancePreFlopSimTask(playerHoleCards, numOfOpponents, numberOfSimulatedGames)
	if err != nil {
		return Chance{}, err
	}

	return Chance{WinChance: float64(wins) / float64(numberOfSimulatedGames), TieChance: float64(ties) / float64(numberOfSimulatedGames)}, nil
}

// Returns Value from 0 to 1.0 from pre-computed data
func GetWinningChancePreFlopLookUp(playerHoleCards Pair, numOfOpponents int, preFlopDataLoader PreFlopDataLoader) (Chance, error) {
	if err := argsPreFlopLookUp(playerHoleCards, numOfOpponents); err != nil {
		return Chance{}, err
	}

	preFlopLookUpTable, err := preFlopDataLoader.Load()
	if err != nil {
		return Chance{}, err
	}

	notation := compactCardPrintLookUp[playerHoleCards.First.Rank] + compactCardPrintLookUp[playerHoleCards.Second.Rank]
	if playerHoleCards.First.Suit == playerHoleCards.Second.Suit {
		notation += "s"
	} else {
		notation += "o"
	}

	result, ok := preFlopLookUpTable[PreFlopKey{HoleCardsInNotation: notation, OpponentCount: numOfOpponents}]
	if !ok {
		return Chance{}, fmt.Errorf("there is most likely no pre-computed data for numOfOpponents = %d", numOfOpponents)
	}

	return result, nil
}

// Returns Value from 0 to 1.0 | Realistically: 0.1166 to 0.8389
func GetWinningChancePreFlopChen(playerHoleCards Pair) (float64, error) {
	if err := againstDuplicateHoleCards(playerHoleCards, "playerHoleCards"); err != nil {
		return 0, err
	}

	chen, err := GetPreFlopChen(playerHoleCards)
	if err != nil {
		return 0, err
	}

	// ! Sigmoid adjustment
	return 1 / (1 + math.Exp(-(handStrengthSensitivity*chen + baselineWinRate))), nil
}

// Returns -1 to 20
func GetPreFlopChen(playerHoleCards Pair) (float64, error) {
	if err := againstDuplicateHoleCards(playerHoleCards, "playerHoleCards"); err != nil {
		return 0, err
	}

	points := 0.0
	var higherCard Card
	var lowerCard Card

	if playerHoleCards.First.Rank > playerHoleCards.Second.Rank {
		higherCard = playerHoleCards.First
		lowerCard = playerHoleCards.Second
	} else {
		higherCard = playerHoleCards.Second
		lowerCard = playerHoleCards.First
	}

	switch higherCard.Rank {
	case 14:
		points += 10
	case 13:
		points += 8
	case 12:
		points += 7
	case 11:
		points += 6
	default:
		points += float64(higherCard.Rank) / 2
	}

	if higherCard.Rank == lowerCard.Rank {
		points *= 2
		if points < 5 {
			points = 5
		}
	}

	if higherCard.Suit == lowerCard.Suit {
		points += 2
	}

	gap := 0
	if higherCard.Rank != lowerCard.Rank {
		gap = int(math.Abs(float64(higherCard.Rank - lowerCard.Rank - 1)))
	}

	if gap >= 4 {
		points -= 5
	} else if gap == 3 {
		points -= 4
	} else {
		points -= float64(gap)
	}

	if (gap == 0 || gap == 1) && higherCard.Rank != lowerCard.Rank && higherCard.Rank < 12 && lowerCard.Rank < 12 {
		points += 1
	}

	if points == -1.5 {
		points = -1
	} else if points == -0.5 {
		points = 0
	} else {
		points = math.Round(points)
	}

	if points < -1 {
		return 0, newError(ErrInternalPokerAlgo, "invariant violated: points should always be greater than -1 before returning")
	}

	return points, nil
}

func winningChanceSimTask(holeCards Pair, communityCards []Card, numOfOpponents int, sims int) (int, int, error) {
	testDeck := NewDeck()
	timesWon := 0
	timesTied := 0

	player := NewPlayer("Player", holeCards.First, holeCards.Second)
	cardsToRemove := []Card{player.HoleCards.First, player.HoleCards.Second}
	cardsToRemove = append(cardsToRemove, communityCards...)
	remainingCommunity := 5 - len(communityCards)

	allPlayers := []Player{player}
	for k := 0; k < numOfOpponents; k++ {
		allPlayers = append(allPlayers, NewPlayer("Simulated Opponent", testDeck.MustNextCard(), testDeck.MustNextCard()))
	}

	for i := 0; i < sims; i++ {
		testDeck.ResetDeck()
		if err := testDeck.RemoveCards(cardsToRemove); err != nil {
			return 0, 0, err
		}

		for playerIndex := range allPlayers {
			if allPlayers[playerIndex].Name != "Player" {
				allPlayers[playerIndex].NewHand(testDeck.MustNextCard(), testDeck.MustNextCard())
			}
		}

		fullCommunity := slices.Clone(communityCards)
		if remainingCommunity > 0 {
			fullCommunity = append(fullCommunity, testDeck.MustNextCards(remainingCommunity)...)
		}

		winners, err := GetWinners(allPlayers, fullCommunity)
		if err != nil {
			return 0, 0, err
		}

		if len(winners) == 1 && samePlayer(winners[0], player) {
			timesWon++
		} else if len(winners) > 1 && containsPlayer(winners, player) {
			timesTied++
		}
	}

	return timesWon, timesTied, nil
}

func winningChancePreFlopSimTask(holeCards Pair, numOfOpponents int, sims int) (int, int, error) {
	testDeck := NewDeck()
	timesWon := 0
	timesTied := 0

	player := NewPlayer("Player", holeCards.First, holeCards.Second)
	cardsToRemove := []Card{player.HoleCards.First, player.HoleCards.Second}

	allPlayers := []Player{player}
	for k := 0; k < numOfOpponents; k++ {
		allPlayers = append(allPlayers, NewPlayer("Simulated Opponent", testDeck.MustNextCard(), testDeck.MustNextCard()))
	}

	for i := 0; i < sims; i++ {
		testDeck.ResetDeck()
		if err := testDeck.RemoveCards(cardsToRemove); err != nil {
			return 0, 0, err
		}

		for playerIndex := range allPlayers {
			if allPlayers[playerIndex].Name != "Player" {
				allPlayers[playerIndex].NewHand(testDeck.MustNextCard(), testDeck.MustNextCard())
			}
		}

		communityCards := testDeck.MustNextCards(5)
		winners, err := GetWinners(allPlayers, communityCards)
		if err != nil {
			return 0, 0, err
		}

		if len(winners) == 1 && samePlayer(winners[0], player) {
			timesWon++
		} else if len(winners) > 1 && containsPlayer(winners, player) {
			timesTied++
		}
	}

	return timesWon, timesTied, nil
}

func containsPlayer(players []Player, target Player) bool {
	for _, player := range players {
		if samePlayer(player, target) {
			return true
		}
	}
	return false
}
