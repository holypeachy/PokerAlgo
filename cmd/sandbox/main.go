package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"log"
	"os"
	"path/filepath"
	"strings"
	"time"

	pokeralgo "pokeralgo"
	"pokeralgo/internal/preflopcompute"
)

const numOfCommunityCards = 5

var playerNames = []string{"Tom", "Matt", "Ben", "Sam", "Jim"}
var sandboxLogger = log.New(os.Stdout, "", 0)

func main() {
	mode := flag.String("mode", "", "mode: main, sim, chen, lookup, manual, compute, template-hand, template-algo")
	sims := flag.Int("sims", 0, "number of simulations")
	seed := flag.Int64("seed", 0, "deck seed; 0 uses a random seed")
	preflopDir := flag.String("preflop-dir", "", "directory containing .preflop files")
	outDir := flag.String("out", "", "output directory for compute/template modes")
	opponents := flag.Int("opponents", 0, "number of opponents for compute mode")
	debug := flag.String("debug", "off", "debug level: off, summary, trace")
	flag.Parse()

	if err := pokeralgo.SetDebugLevel(*debug); err != nil {
		fmt.Fprintf(os.Stderr, "⚠️ [sandbox] %v\n", err)
		flag.Usage()
		os.Exit(1)
	}

	if !flagProvided("mode") {
		fmt.Fprintln(os.Stderr, "⚠️ [sandbox] please provide -mode")
		flag.Usage()
		os.Exit(1)
	}
	if err := validateModeFlags(*mode); err != nil {
		fmt.Fprintf(os.Stderr, "⚠️ [sandbox] %v\n", err)
		flag.Usage()
		os.Exit(1)
	}

	started := time.Now()

	deck := newDeck(*seed)
	players := makePlayers(deck)
	communityCards := deck.MustDrawN(numOfCommunityCards)

	switch *mode {
	case "main":
		printGameStart(players, communityCards, *preflopDir)
		if err := mainExecution(players, communityCards); err != nil {
			exitErr(err)
		}
	case "sim":
		printGameStart(players, communityCards, *preflopDir)
		if err := monteCarloSim(players, communityCards, *sims); err != nil {
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
		fmt.Fprintln(os.Stderr, "⚠️ [sandbox] please enter a valid -mode")
		flag.Usage()
		os.Exit(1)
	}

	sandboxLogger.Printf("\n🕜 [sandbox] execution time: %s", time.Since(started).Round(time.Millisecond))
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
		players = append(players, pokeralgo.NewPlayer(name, deck.MustDraw(), deck.MustDraw()))
	}
	return players
}

func printGameStart(players []pokeralgo.Player, communityCards []pokeralgo.Card, preflopDir string) {
	loader := pokeralgo.NewFolderLoader(preflopDir)
	var output strings.Builder
	output.WriteString("🚀 [sandbox] game started\n👥 [sandbox] players")
	for _, player := range players {
		chance, err := pokeralgo.LookupPreflop(player.HoleCards, len(players)-1, loader)
		if err != nil {
			fmt.Fprintf(&output, "\n   ??.??%% | %s", player)
			continue
		}
		fmt.Fprintf(&output, "\n   %0.2f%% | %s", chance.Win*100, player)
	}
	sandboxLogger.Print(output.String())

	sandboxLogger.Printf("🃏 [sandbox] community cards\n   %s", cardsToString(communityCards))
}

func monteCarloSim(players []pokeralgo.Player, communityCards []pokeralgo.Card, sims int) error {
	sandboxLogger.Printf("\n🎲 [simulation] running\n   simulations: %d", sims)

	for _, player := range players {
		winningHand, err := pokeralgo.EvaluatePlayer(player.HoleCards, communityCards)
		if err != nil {
			return err
		}
		player.BestHand = &winningHand

		chance, err := pokeralgo.Simulate(player.HoleCards, communityCards, len(players)-1, sims)
		if err != nil {
			return err
		}

		sandboxLogger.Printf(
			"👤 [simulation] %s\n   win: %0.2f%%\n   tie: %0.2f%%",
			formatPlayerHand(player),
			chance.Win*100,
			chance.Tie*100,
		)
	}

	return nil
}

func chenPreFlopChances(players []pokeralgo.Player) error {
	sandboxLogger.Print("🧮 [chen] preflop estimates")
	for _, player := range players {
		chen, err := pokeralgo.ChenScore(player.HoleCards)
		if err != nil {
			return err
		}
		chance, err := pokeralgo.ChenEstimate(player.HoleCards)
		if err != nil {
			return err
		}

		sandboxLogger.Printf("   %s | chen=%v | win=%0.2f%%", player, chen, chance*100)
	}

	samples := []struct {
		name  string
		cards pokeralgo.HoleCards
	}{
		{"AAo", pokeralgo.HoleCards{First: pokeralgo.MustCard(14, pokeralgo.Spades, true), Second: pokeralgo.MustCard(14, pokeralgo.Diamonds, true)}},
		{"KAs", pokeralgo.HoleCards{First: pokeralgo.MustCard(13, pokeralgo.Spades, true), Second: pokeralgo.MustCard(14, pokeralgo.Spades, true)}},
		{"27o", pokeralgo.HoleCards{First: pokeralgo.MustCard(2, pokeralgo.Spades, true), Second: pokeralgo.MustCard(7, pokeralgo.Diamonds, true)}},
	}

	sandboxLogger.Print("🧪 [chen] reference hands")
	for _, sample := range samples {
		chen, err := pokeralgo.ChenScore(sample.cards)
		if err != nil {
			return err
		}
		chance, err := pokeralgo.ChenEstimate(sample.cards)
		if err != nil {
			return err
		}

		sandboxLogger.Printf("   %s | chen=%v | win=%0.2f%%", sample.name, chen, chance*100)
	}

	return nil
}

func lookupPreFlopChances(players []pokeralgo.Player, preflopDir string) error {
	loader := pokeralgo.NewFolderLoader(preflopDir)

	sandboxLogger.Print("📚 [lookup] preflop chances")
	for _, player := range players {
		chance, err := pokeralgo.LookupPreflop(player.HoleCards, len(players)-1, loader)
		if err != nil {
			return err
		}

		sandboxLogger.Printf("   %s | win=%0.2f%% | tie=%0.2f%%", player, chance.Win*100, chance.Tie*100)
	}

	return nil
}

func preFlopComputation(opponents int, sims int, outDir string) error {
	if outDir == "" {
		return fmt.Errorf("please enter a valid directory path")
	}

	sandboxLogger.Printf(
		"💭 [compute] generating preflop data\n   opponents: %d\n   simulations per hand: %d\n   output: %s",
		opponents,
		sims,
		outDir,
	)

	return preflopcompute.Run(preflopcompute.Options{
		MaxOpponents: opponents,
		Sims:         sims,
		OutDir:       outDir,
	})
}

func mainExecution(players []pokeralgo.Player, communityCards []pokeralgo.Card) error {
	winners, err := pokeralgo.DetermineWinners(players, communityCards)
	if err != nil {
		return err
	}

	var output strings.Builder
	output.WriteString("🥇 [result] winners")
	for _, player := range winners {
		fmt.Fprintf(&output, "\n   %s", formatPlayerHand(player))
	}
	sandboxLogger.Print(output.String())
	return nil
}

func manual(players []pokeralgo.Player, communityCards []pokeralgo.Card) error {
	if len(players) == 0 {
		return fmt.Errorf("no players")
	}

	timer := time.Now()

	winningHand, err := pokeralgo.EvaluatePlayer(players[0].HoleCards, communityCards)
	if err != nil {
		return err
	}
	sandboxLogger.Printf(
		"🧪 [manual] hand\n   player: %s\n   board: %s\n   result: %s",
		players[0],
		cardsToString(communityCards),
		pokeralgo.DescribeHand(winningHand),
	)

	sandboxLogger.Print("🎲 [manual] postflop simulation\n   opponents: 4\n   simulations: 1000000")
	started := time.Now()
	result, err := pokeralgo.Simulate(players[0].HoleCards, communityCards, 4, 1_000_000)
	if err != nil {
		return err
	}
	sandboxLogger.Printf(
		"   win=%v | tie=%v | time=%s",
		result.Win,
		result.Tie,
		time.Since(started).Round(time.Millisecond),
	)

	sandboxLogger.Print("🎲 [manual] preflop simulation\n   opponents: 4\n   simulations: 1000000")
	started = time.Now()
	result, err = pokeralgo.SimulatePreflop(players[0].HoleCards, 4, 1_000_000)
	if err != nil {
		return err
	}
	sandboxLogger.Printf(
		"   win=%v | tie=%v | time=%s",
		result.Win,
		result.Tie,
		time.Since(started).Round(time.Millisecond),
	)

	sandboxLogger.Printf("🕜 [manual] total time: %s", time.Since(timer).Round(time.Millisecond))
	return nil
}

func formatPlayerHand(player pokeralgo.Player) string {
	handName := "<nil>"
	cards := ""
	if player.BestHand != nil {
		handName = pokeralgo.DescribeHand(*player.BestHand)
		cards = cardsToString(player.BestHand.Cards)
	}
	return fmt.Sprintf("%s | %s | %s", player.Name, handName, cards)
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
	sandboxLogger.Printf("🧪 [fixture] creating hand template: %q", pathToTest)
	deck := pokeralgo.NewDeck()
	community := deck.MustDrawN(5)
	playerHand := pokeralgo.HoleCards{First: deck.MustDraw(), Second: deck.MustDraw()}
	winning := pokeralgo.Hand{Type: pokeralgo.HighCard, Cards: community}
	test := handEvalTest{
		Description:    "My Description",
		CommunityCards: community,
		PlayerCards:    playerHand,
		ExpectedHand:   winning,
	}

	return writeIndentedJSON(pathToTest, []handEvalTest{test, test})
}

func makeTemplateAlgoTestJSON(pathToTest string) error {
	sandboxLogger.Printf("🧪 [fixture] creating winner template: %q", pathToTest)
	deck := pokeralgo.NewDeck()
	community := deck.MustDrawN(5)
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
	sandboxLogger.Printf("✅ [fixture] created: %q", path)
	return nil
}

func exitErr(err error) {
	fmt.Fprintf(os.Stderr, "⛔ [sandbox] %v\n", err)
	os.Exit(1)
}

type handEvalTest struct {
	Description    string
	PlayerCards    pokeralgo.HoleCards
	CommunityCards []pokeralgo.Card
	ExpectedHand   pokeralgo.Hand
}

type algoTest struct {
	Description      string
	Player1          pokeralgo.HoleCards
	Player2          pokeralgo.HoleCards
	Player3          pokeralgo.HoleCards
	CommunityCards   []pokeralgo.Card
	IndicesOfWinners []int
}

/*
! ISSUES:
!

TODO
TODO:

? Future Ideas
? Generate a ton of data on the Monte Carlo sims and find how many simulations give the most accurate prediction while minimizing compute time.
? Remove symmetric entries on the preflop computation logic. AKo == KAo

? Simulate all players together for accurate chances of winning that add to 100%.
? Precompute post-flop chances of winning? ( Would probably take days of CPU time :< )
? Modular Architecture: Make Player and Card an interface. Make Deck generic. (Is this necessary or useful?)
? Better IO handling: FolderLoader rejecting badly formatted lines and badly formatted file names.
? Add meta data to pre-flop calculations
? Add path to pre-flop to a single location, like an environmental variable

* Notes
* Reset() THEN Exclude() together, always before using Draw().
* A Deck keeps one seed and one RNG for its entire lifetime. Reset() continues that deterministic RNG stream.
* NewDeck() uses a time-based seed for convenience. Use GenerateSeed() and NewDeckWithSeed() for a real game and replay.
* One game seed reproduces every shuffle. To reach a later hand, recreate the Deck and advance it with Reset().
* For bulk compute, GOGC=1000 trades roughly 190-237 MB of memory for much higher CPU utilization and substantially lower execution time.

* Changes
* Renamed chance_calculator.go to simulations.go and shortened the public simulation, lookup, and Chen APIs.
* Renamed hand_evaluator.go to evaluator.go and standardized the validation helper names.
*/
