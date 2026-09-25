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
func Simulate(playerHoleCards HoleCards, communityCards []Card, numOfOpponents int, numberOfSimulatedGames int) (Chance, error) {
	if err := validateSimulation(playerHoleCards, communityCards, numOfOpponents, numberOfSimulatedGames); err != nil {
		return Chance{}, err
	}

	numberOfJobs := runtime.NumCPU()
	simulationsPerJob := numberOfSimulatedGames / numberOfJobs
	remainder := numberOfSimulatedGames % numberOfJobs

	type result struct {
		wins int
		ties int
		err  error
	}

	results := make(chan result, numberOfJobs)
	var waitGroup sync.WaitGroup
	for jobIndex := range numberOfJobs {
		simulations := simulationsPerJob
		if jobIndex == 0 {
			simulations += remainder
		}

		waitGroup.Add(1)
		go func() {
			defer waitGroup.Done()
			wins, ties, err := runSimulationJob(playerHoleCards, communityCards, numOfOpponents, simulations)
			results <- result{wins: wins, ties: ties, err: err}
		}()
	}

	waitGroup.Wait()
	close(results)

	totalWins := 0
	totalTies := 0
	for jobResult := range results {
		if jobResult.err != nil {
			return Chance{}, jobResult.err
		}
		totalWins += jobResult.wins
		totalTies += jobResult.ties
	}

	return Chance{
		Win: float64(totalWins) / float64(numberOfSimulatedGames),
		Tie: float64(totalTies) / float64(numberOfSimulatedGames),
	}, nil
}

// Returns Win and Tie Values from 0 to 1.0
func SimulatePreflop(playerHoleCards HoleCards, numOfOpponents int, numberOfSimulatedGames int) (Chance, error) {
	if err := validatePreflopSimulation(playerHoleCards, numOfOpponents, numberOfSimulatedGames); err != nil {
		return Chance{}, err
	}

	numberOfJobs := runtime.NumCPU()
	simulationsPerJob := numberOfSimulatedGames / numberOfJobs
	remainder := numberOfSimulatedGames % numberOfJobs

	type result struct {
		wins int
		ties int
		err  error
	}

	results := make(chan result, numberOfJobs)
	var waitGroup sync.WaitGroup
	for jobIndex := range numberOfJobs {
		simulations := simulationsPerJob
		if jobIndex == 0 {
			simulations += remainder
		}

		waitGroup.Add(1)
		go func() {
			defer waitGroup.Done()
			wins, ties, err := runPreflopSimulationJob(playerHoleCards, numOfOpponents, simulations)
			results <- result{wins: wins, ties: ties, err: err}
		}()
	}

	waitGroup.Wait()
	close(results)

	totalWins := 0
	totalTies := 0
	for jobResult := range results {
		if jobResult.err != nil {
			return Chance{}, jobResult.err
		}
		totalWins += jobResult.wins
		totalTies += jobResult.ties
	}

	return Chance{
		Win: float64(totalWins) / float64(numberOfSimulatedGames),
		Tie: float64(totalTies) / float64(numberOfSimulatedGames),
	}, nil
}

// Returns Value from 0 to 1.0 from pre-computed data
func LookupPreflop(playerHoleCards HoleCards, numOfOpponents int, preFlopDataLoader PreFlopDataLoader) (Chance, error) {
	if err := validatePreflopLookup(playerHoleCards, numOfOpponents); err != nil {
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
		return Chance{}, newError(ErrPreFlopDataNotFound, fmt.Sprintf("there is most likely no pre-computed data for numOfOpponents = %d", numOfOpponents))
	}

	return result, nil
}

// Returns Value from 0 to 1.0 | Realistically: 0.1166 to 0.8389
func ChenEstimate(playerHoleCards HoleCards) (float64, error) {
	if err := validateHoleCards(playerHoleCards, "playerHoleCards"); err != nil {
		return 0, err
	}

	chen, err := ChenScore(playerHoleCards)
	if err != nil {
		return 0, err
	}

	// ! Sigmoid adjustment
	return 1 / (1 + math.Exp(-(handStrengthSensitivity*chen + baselineWinRate))), nil
}

// Returns -1 to 20
func ChenScore(playerHoleCards HoleCards) (float64, error) {
	if err := validateHoleCards(playerHoleCards, "playerHoleCards"); err != nil {
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

func runSimulationJob(holeCards HoleCards, communityCards []Card, numOfOpponents int, sims int) (int, int, error) {
	testDeck := NewDeck()
	timesWon := 0
	timesTied := 0

	player := NewPlayer("Player", holeCards.First, holeCards.Second)
	cardsToRemove := []Card{player.HoleCards.First, player.HoleCards.Second}
	cardsToRemove = append(cardsToRemove, communityCards...)
	remainingCommunity := 5 - len(communityCards)

	allPlayers := []Player{player}
	for k := 0; k < numOfOpponents; k++ {
		allPlayers = append(allPlayers, NewPlayer("Simulated Opponent", testDeck.MustDraw(), testDeck.MustDraw()))
	}

	for i := 0; i < sims; i++ {
		testDeck.Reset()
		if err := testDeck.Exclude(cardsToRemove); err != nil {
			return 0, 0, err
		}

		for playerIndex := range allPlayers {
			if allPlayers[playerIndex].Name != "Player" {
				allPlayers[playerIndex].SetHoleCards(testDeck.MustDraw(), testDeck.MustDraw())
			}
		}

		fullCommunity := slices.Clone(communityCards)
		if remainingCommunity > 0 {
			fullCommunity = append(fullCommunity, testDeck.MustDrawN(remainingCommunity)...)
		}

		winners, err := DetermineWinners(allPlayers, fullCommunity)
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

func runPreflopSimulationJob(holeCards HoleCards, numOfOpponents int, sims int) (int, int, error) {
	testDeck := NewDeck()
	timesWon := 0
	timesTied := 0

	player := NewPlayer("Player", holeCards.First, holeCards.Second)
	cardsToRemove := []Card{player.HoleCards.First, player.HoleCards.Second}

	allPlayers := []Player{player}
	for k := 0; k < numOfOpponents; k++ {
		allPlayers = append(allPlayers, NewPlayer("Simulated Opponent", testDeck.MustDraw(), testDeck.MustDraw()))
	}

	for i := 0; i < sims; i++ {
		testDeck.Reset()
		if err := testDeck.Exclude(cardsToRemove); err != nil {
			return 0, 0, err
		}

		for playerIndex := range allPlayers {
			if allPlayers[playerIndex].Name != "Player" {
				allPlayers[playerIndex].SetHoleCards(testDeck.MustDraw(), testDeck.MustDraw())
			}
		}

		communityCards := testDeck.MustDrawN(5)
		winners, err := DetermineWinners(allPlayers, communityCards)
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
