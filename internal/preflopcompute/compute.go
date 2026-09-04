package preflopcompute

import (
	"bufio"
	"fmt"
	"os"
	"path/filepath"

	pokeralgo "pokeralgo"
)

type Options struct {
	MaxOpponents int
	Sims         int
	OutDir       string
}

type StartingHand struct {
	Notation string
	Cards    pokeralgo.Pair
}

var cardPrintLookUp = map[int]string{
	1: "A", 2: "2", 3: "3", 4: "4", 5: "5", 6: "6", 7: "7",
	8: "8", 9: "9", 10: "T", 11: "J", 12: "Q", 13: "K", 14: "A",
}

func Run(options Options) error {
	if options.MaxOpponents < 1 || options.Sims < 100 {
		return fmt.Errorf("please use opponents >= 1 and sims >= 100")
	}
	if err := os.MkdirAll(options.OutDir, 0o755); err != nil {
		return err
	}

	hands := BuildStartingHands()

	for currentOpponents := 1; currentOpponents <= options.MaxOpponents; currentOpponents++ {
		fmt.Printf("Current Number of Opponents: %d\n", currentOpponents)

		filePath := filepath.Join(options.OutDir, fmt.Sprintf("%d_%d.preflop", currentOpponents, options.Sims))
		file, err := os.Create(filePath)
		if err != nil {
			return fmt.Errorf("write %q: %w", filePath, err)
		}

		writer := bufio.NewWriter(file)
		for index, hand := range hands {
			chance, err := pokeralgo.GetWinningChancePreFlopSimParallel(hand.Cards, currentOpponents, options.Sims)
			if err != nil {
				file.Close()
				return err
			}

			if _, err := fmt.Fprintf(writer, "%s %v %v\n", hand.Notation, chance.WinChance, chance.TieChance); err != nil {
				file.Close()
				return err
			}

			fmt.Printf("\rProgress: %d/%d", index+1, len(hands))
		}

		if err := writer.Flush(); err != nil {
			file.Close()
			return err
		}
		if err := file.Close(); err != nil {
			return err
		}

		fmt.Printf("\n✅ Computations done. Data can be found in: %s\n\n", filePath)
	}

	fmt.Println("🗯️ The data generated can be used by the default PokerAlgo Loader (FolderLoader).")
	return nil
}

func BuildStartingHands() []StartingHand {
	hands := make([]StartingHand, 0, 338)

	for i := 2; i <= 14; i++ {
		for j := 2; j <= 14; j++ {
			first := pokeralgo.MustCard(i, pokeralgo.Spades, true)
			second := pokeralgo.MustCard(j, pokeralgo.Hearts, true)
			hands = append(hands, StartingHand{
				Notation: cardPrintLookUp[first.Rank] + cardPrintLookUp[second.Rank] + "o",
				Cards:    pokeralgo.Pair{First: first, Second: second},
			})
		}
	}

	for i := 2; i <= 14; i++ {
		for j := 2; j <= 14; j++ {
			if i == j {
				continue
			}

			first := pokeralgo.MustCard(i, pokeralgo.Hearts, true)
			second := pokeralgo.MustCard(j, pokeralgo.Hearts, true)
			hands = append(hands, StartingHand{
				Notation: cardPrintLookUp[first.Rank] + cardPrintLookUp[second.Rank] + "s",
				Cards:    pokeralgo.Pair{First: first, Second: second},
			})
		}
	}

	return hands
}
