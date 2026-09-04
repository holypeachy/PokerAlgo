package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"os"
	"path/filepath"
	"time"

	pokeralgo "pokeralgo"
	"pokeralgo/internal/preflopcompute"
)

const numOfCommunityCards = 5

var playerNames = []string{"Tom", "Matt", "Ben", "Sam", "Jim"}

func main() {
	mode := flag.String("mode", "", "mode: main, sim, chen, lookup, manual, compute, template-hand, template-algo")
	sims := flag.Int("sims", 0, "number of simulations")
	seed := flag.Int64("seed", 0, "deck seed; 0 uses a random seed")
	preflopDir := flag.String("preflop-dir", "", "directory containing .preflop files")
	outDir := flag.String("out", "", "output directory for compute/template modes")
	opponents := flag.Int("opponents", 0, "number of opponents for compute mode")
	parallel := flag.Bool("parallel", true, "use parallel simulations where applicable")
	flag.Parse()

	if !flagProvided("mode") {
		fmt.Fprintln(os.Stderr, "⚠️ pokeralgo-sandbox: please provide -mode")
		flag.Usage()
		os.Exit(1)
	}
	if err := validateModeFlags(*mode); err != nil {
		fmt.Fprintf(os.Stderr, "⚠️ pokeralgo-sandbox: %v\n", err)
		flag.Usage()
		os.Exit(1)
	}

	started := time.Now()

	deck := newDeck(*seed)
	players := makePlayers(deck)
	communityCards := deck.MustNextCards(numOfCommunityCards)

	switch *mode {
	case "main":
		printGameStart(players, communityCards, *preflopDir)
		if err := mainExecution(players, communityCards); err != nil {
			exitErr(err)
		}
	case "sim":
		printGameStart(players, communityCards, *preflopDir)
		if err := monteCarloSim(players, communityCards, *sims, *parallel); err != nil {
			exitErr(err)
		}
	case "chen":
		if err := chenPreFlopChances(players); err != nil {
			exitErr(err)
		}
	case "lookup":
		if err := lookupPreFlopChances(players, *preflopDir); err != nil {
			exitErr(err)
		}
	case "manual":
		if err := manual(players, communityCards); err != nil {
			exitErr(err)
		}
	case "compute":
		if err := preFlopComputation(*opponents, *sims, *outDir); err != nil {
			exitErr(err)
		}
	case "template-hand":
		if err := makeTemplateHandEvalTestJSON(filepath.Join(*outDir, "HandEvalUnitTests.json")); err != nil {
			exitErr(err)
		}
	case "template-algo":
		if err := makeTemplateAlgoTestJSON(filepath.Join(*outDir, "AlgoTests.json")); err != nil {
			exitErr(err)
		}
	default:
		fmt.Fprintln(os.Stderr, "⚠️ pokeralgo-sandbox: please enter a valid -mode")
		flag.Usage()
		os.Exit(1)
	}

	fmt.Printf("\n🕜 Execution Time: %s\n", time.Since(started).Round(time.Millisecond))
}

func validateModeFlags(mode string) error {
	switch mode {
	case "main", "lookup":
		if !flagProvided("preflop-dir") {
			return fmt.Errorf("mode %q requires -preflop-dir", mode)
		}
	case "sim":
		if !flagProvided("sims") || !flagProvided("preflop-dir") {
			return fmt.Errorf("mode %q requires -sims and -preflop-dir", mode)
		}
	case "compute":
		if !flagProvided("opponents") || !flagProvided("sims") || !flagProvided("out") {
			return fmt.Errorf("mode %q requires -opponents, -sims, and -out", mode)
		}
	case "template-hand", "template-algo":
		if !flagProvided("out") {
			return fmt.Errorf("mode %q requires -out", mode)
		}
	case "chen", "manual":
		return nil
	case "":
		return fmt.Errorf("please provide -mode")
	default:
		return fmt.Errorf("please enter a valid -mode")
	}
	return nil
}

func flagProvided(name string) bool {
	found := false
	flag.Visit(func(f *flag.Flag) {
		if f.Name == name {
			found = true
		}
	})
	return found
}

func newDeck(seed int64) *pokeralgo.Deck {
	if seed == 0 {
		return pokeralgo.NewDeck()
	}
	return pokeralgo.NewDeckWithSeed(seed)
}

func makePlayers(deck *pokeralgo.Deck) []pokeralgo.Player {
	players := make([]pokeralgo.Player, 0, len(playerNames))
	for _, name := range playerNames {
		players = append(players, pokeralgo.NewPlayer(name, deck.MustNextCard(), deck.MustNextCard()))
	}
	return players
}

func printGameStart(players []pokeralgo.Player, communityCards []pokeralgo.Card, preflopDir string) {
	fmt.Println("--- 🚀 Game Starts")
	fmt.Println("--- 😎 Players:")

	loader := pokeralgo.NewFolderLoader(preflopDir)
	for _, player := range players {
		chance, err := pokeralgo.GetWinningChancePreFlopLookUp(player.HoleCards, len(players)-1, loader)
		if err != nil {
			fmt.Printf("\t??.??%% - %s\n", player)
			continue
		}
		fmt.Printf("\t%0.2f%% - %s\n", chance.WinChance*100, player)
	}

	fmt.Print("\n--- 🃏 Community Cards:\n\t\t")
	for _, card := range communityCards {
		fmt.Printf("%s ", card)
	}
	fmt.Println()
	fmt.Println()
}

func monteCarloSim(players []pokeralgo.Player, communityCards []pokeralgo.Card, sims int, parallel bool) error {
	fmt.Println()
	fmt.Printf("Number of Simulations: %d\n", sims)
	fmt.Println("-----------------------------")

	for _, player := range players {
		winningHand, err := pokeralgo.GetWinningHandForPlayer(player.HoleCards, communityCards)
		if err != nil {
			return err
		}
		player.WinningHand = &winningHand

		printPlayerHand(player)

		chance, err := winningChanceSim(player.HoleCards, communityCards, len(players)-1, sims, parallel)
		if err != nil {
			return err
		}

		fmt.Printf("\tWin: %0.2f%%\n", chance.WinChance*100)
		fmt.Printf("\tTie: %0.2f%%\n\n", chance.TieChance*100)
	}

	return nil
}

func chenPreFlopChances(players []pokeralgo.Player) error {
	fmt.Println("Chen + Sigmoid Pre-Flop")
	fmt.Println("-----------------------------")
	for _, player := range players {
		chen, err := pokeralgo.GetPreFlopChen(player.HoleCards)
		if err != nil {
			return err
		}
		chance, err := pokeralgo.GetWinningChancePreFlopChen(player.HoleCards)
		if err != nil {
			return err
		}

		fmt.Println(player)
		fmt.Printf("\tChen: %v\n", chen)
		fmt.Printf("\tWin: %0.2f%%\n", chance*100)
	}

	samples := []struct {
		name  string
		cards pokeralgo.Pair
	}{
		{"AAo", pokeralgo.Pair{First: pokeralgo.MustCard(14, pokeralgo.Spades, true), Second: pokeralgo.MustCard(14, pokeralgo.Diamonds, true)}},
		{"KAs", pokeralgo.Pair{First: pokeralgo.MustCard(13, pokeralgo.Spades, true), Second: pokeralgo.MustCard(14, pokeralgo.Spades, true)}},
		{"27o", pokeralgo.Pair{First: pokeralgo.MustCard(2, pokeralgo.Spades, true), Second: pokeralgo.MustCard(7, pokeralgo.Diamonds, true)}},
	}

	fmt.Println()
	for _, sample := range samples {
		chen, err := pokeralgo.GetPreFlopChen(sample.cards)
		if err != nil {
			return err
		}
		chance, err := pokeralgo.GetWinningChancePreFlopChen(sample.cards)
		if err != nil {
			return err
		}

		fmt.Printf("%s\n  Chen: %v\n  Win: %0.2f%%\n", sample.name, chen, chance*100)
	}

	return nil
}

func lookupPreFlopChances(players []pokeralgo.Player, preflopDir string) error {
	loader := pokeralgo.NewFolderLoader(preflopDir)

	fmt.Println("Lookup Pre-Flop Chances")
	fmt.Println("-----------------------------")
	for _, player := range players {
		chance, err := pokeralgo.GetWinningChancePreFlopLookUp(player.HoleCards, len(players)-1, loader)
		if err != nil {
			return err
		}

		fmt.Println(player)
		fmt.Printf("\tWin: %0.2f%%\n", chance.WinChance*100)
		fmt.Printf("\tTie: %0.2f%%\n", chance.TieChance*100)
	}
	fmt.Println()

	return nil
}

func preFlopComputation(opponents int, sims int, outDir string) error {
	if outDir == "" {
		return fmt.Errorf("please enter a valid directory path")
	}

	fmt.Println("💭 Computing chances of winning for all starting hands...")
	fmt.Printf("Simulations per Hand: %d\n", sims)

	return preflopcompute.Run(preflopcompute.Options{
		MaxOpponents: opponents,
		Sims:         sims,
		OutDir:       outDir,
	})
}

func mainExecution(players []pokeralgo.Player, communityCards []pokeralgo.Card) error {
	winners, err := pokeralgo.GetWinners(players, communityCards)
	if err != nil {
		return err
	}

	fmt.Println("🥇 Program.Main() Winner(s):")
	for _, player := range winners {
		printPlayerHand(player)
	}
	return nil
}

func manual(players []pokeralgo.Player, communityCards []pokeralgo.Card) error {
	if len(players) == 0 {
		return fmt.Errorf("no players")
	}

	timer := time.Now()

	fmt.Println(players[0])
	fmt.Println()
	for _, card := range communityCards {
		fmt.Printf("%s ", card)
	}
	fmt.Println()

	winningHand, err := pokeralgo.GetWinningHandForPlayer(players[0].HoleCards, communityCards)
	if err != nil {
		return err
	}
	fmt.Println()
	fmt.Println(winningHand)
	fmt.Println()

	fmt.Println("4 Opponents, 1 Million Sims")
	fmt.Println()
	started := time.Now()
	result, err := pokeralgo.GetWinningChanceSim(players[0].HoleCards, communityCards, 4, 1_000_000)
	if err != nil {
		return err
	}
	fmt.Printf("Single-Thread:\n\twin: %v\n\ttie: %v\n\ttime: %s\n", result.WinChance, result.TieChance, time.Since(started).Round(time.Millisecond))

	started = time.Now()
	result, err = pokeralgo.GetWinningChanceSimParallel(players[0].HoleCards, communityCards, 4, 1_000_000)
	if err != nil {
		return err
	}
	fmt.Printf("Multi-Thread:\n\twin: %v\n\ttie: %v\n\ttime: %s\n", result.WinChance, result.TieChance, time.Since(started).Round(time.Millisecond))

	fmt.Println("\nPre-Flop")
	fmt.Println("4 Opponents, 1 Million Sims")
	fmt.Println()
	started = time.Now()
	result, err = pokeralgo.GetWinningChancePreFlopSim(players[0].HoleCards, 4, 1_000_000)
	if err != nil {
		return err
	}
	fmt.Printf("Single-Thread:\n\twin: %v\n\ttie: %v\n\ttime: %s\n", result.WinChance, result.TieChance, time.Since(started).Round(time.Millisecond))

	started = time.Now()
	result, err = pokeralgo.GetWinningChancePreFlopSimParallel(players[0].HoleCards, 4, 1_000_000)
	if err != nil {
		return err
	}
	fmt.Printf("Multi-Thread:\n\twin: %v\n\ttie: %v\n\ttime: %s\n", result.WinChance, result.TieChance, time.Since(started).Round(time.Millisecond))

	fmt.Printf("\nManual total time: %s\n", time.Since(timer).Round(time.Millisecond))
	return nil
}

func winningChanceSim(cards pokeralgo.Pair, communityCards []pokeralgo.Card, opponents int, sims int, parallel bool) (pokeralgo.Chance, error) {
	if parallel {
		return pokeralgo.GetWinningChanceSimParallel(cards, communityCards, opponents, sims)
	}
	return pokeralgo.GetWinningChanceSim(cards, communityCards, opponents, sims)
}

func printPlayerHand(player pokeralgo.Player) {
	handName := "<nil>"
	cards := ""
	if player.WinningHand != nil {
		handName = pokeralgo.GetPrettyHandName(*player.WinningHand)
		cards = cardsToString(player.WinningHand.Cards)
	}
	fmt.Printf("\t %s  %s  %s \n", player.Name, handName, cards)
}

func cardsToString(cards []pokeralgo.Card) string {
	result := ""
	for index, card := range cards {
		if index > 0 {
			result += " "
		}
		result += card.String()
	}
	return result
}

func makeTemplateHandEvalTestJSON(pathToTest string) error {
	fmt.Printf("- Making Tests JSON file for: %q\n", pathToTest)
	deck := pokeralgo.NewDeck()
	community := deck.MustNextCards(5)
	playerHand := pokeralgo.Pair{First: deck.MustNextCard(), Second: deck.MustNextCard()}
	winning := pokeralgo.WinningHand{Type: pokeralgo.Nothing, Cards: community}
	test := handEvalTest{
		Description:    "My Description",
		CommunityCards: community,
		PlayerCards:    playerHand,
		ExpectedHand:   winning,
	}

	return writeIndentedJSON(pathToTest, []handEvalTest{test, test})
}

func makeTemplateAlgoTestJSON(pathToTest string) error {
	fmt.Printf("- Making Tests JSON file for: %q\n", pathToTest)
	deck := pokeralgo.NewDeck()
	community := deck.MustNextCards(5)
	players := []pokeralgo.Player{
		pokeralgo.NewPlayer("Test Player 1", pokeralgo.MustCard(14, pokeralgo.Spades, true), pokeralgo.MustCard(14, pokeralgo.Clubs, true)),
		pokeralgo.NewPlayer("Test Player 2", pokeralgo.MustCard(14, pokeralgo.Diamonds, true), pokeralgo.MustCard(14, pokeralgo.Hearts, true)),
		pokeralgo.NewPlayer("Test Player 3", pokeralgo.MustCard(13, pokeralgo.Diamonds, true), pokeralgo.MustCard(13, pokeralgo.Hearts, true)),
	}
	_ = deck
	test := algoTest{
		Description:      "Test",
		Player1:          players[0].HoleCards,
		Player2:          players[1].HoleCards,
		Player3:          players[2].HoleCards,
		CommunityCards:   community,
		IndicesOfWinners: []int{0, 1},
	}

	return writeIndentedJSON(pathToTest, []algoTest{test, test})
}

func writeIndentedJSON(path string, value any) error {
	if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
		return err
	}

	data, err := json.MarshalIndent(value, "", "  ")
	if err != nil {
		return err
	}
	data = append(data, '\n')

	if err := os.WriteFile(path, data, 0o644); err != nil {
		return err
	}
	fmt.Printf("- %q has been created!\n", path)
	return nil
}

func exitErr(err error) {
	fmt.Fprintf(os.Stderr, "⛔ %v\n", err)
	os.Exit(1)
}

type handEvalTest struct {
	Description    string
	PlayerCards    pokeralgo.Pair
	CommunityCards []pokeralgo.Card
	ExpectedHand   pokeralgo.WinningHand
}

type algoTest struct {
	Description      string
	Player1          pokeralgo.Pair
	Player2          pokeralgo.Pair
	Player3          pokeralgo.Pair
	CommunityCards   []pokeralgo.Card
	IndicesOfWinners []int
}

/*
! ISSUES:
!

TODO
TODO:

? Future Ideas
? Semantic Debug Levels. Use an enum for verbosity levels.
? Generate a ton of data on the Monte Carlo sims and find how many simulations give the most accurate prediction while minimizing compute time.
? Remove symmetric entries on the preflop computation logic. AKo == KAo
? Replace hard coded debug logs Class.Method() format by using nameof()

? Simulate all players together for accurate chances of winning that add to 100%.
? Precompute post-flop chances of winning? ( Would probably take days of CPU time :< )
? Modular Architecture: Make Player and Card an interface. Make Deck generic. (Is this necessary or useful?)
? Better IO handling: FolderLoader rejecting badly formatted lines and badly formatted file names.
? Add meta data to pre-flop calculations
? Add path to pre-flop to a single location, like an environmental variable

* Notes
* ResetDeck() THEN RemoveCards() together, always before using NextCard().

* Changes
* switch to go
*/
