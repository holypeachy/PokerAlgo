package main

import (
	"flag"
	"fmt"
	"log"
	"os"
	"time"

	"pokeralgo/internal/preflopcompute"
)

var computeLogger = log.New(os.Stdout, "", 0)

func main() {
	opponents := flag.Int("opponents", 0, "maximum number of opponents to compute, starting at 1")
	sims := flag.Int("sims", 0, "number of Monte Carlo simulations per starting hand")
	outDir := flag.String("out", "", "output directory for .preflop files")
	flag.Parse()

	if !allFlagsProvided("opponents", "sims", "out") {
		fmt.Fprintln(os.Stderr, "⚠️ [compute] please provide required flags")
		flag.Usage()
		os.Exit(1)
	}

	started := time.Now()
	computeLogger.Printf(
		"💭 [compute] generating preflop data\n   opponents: %d\n   simulations per hand: %d\n   parallel: true\n   output: %s",
		*opponents,
		*sims,
		*outDir,
	)

	err := preflopcompute.Run(preflopcompute.Options{
		MaxOpponents: *opponents,
		Sims:         *sims,
		OutDir:       *outDir,
	})
	if err != nil {
		fmt.Fprintf(os.Stderr, "⛔ [compute] %v\n", err)
		flag.Usage()
		os.Exit(1)
	}

	computeLogger.Printf("\n🕜 [compute] execution time: %s", time.Since(started).Round(time.Millisecond))
}

func allFlagsProvided(names ...string) bool {
	seen := make(map[string]bool, len(names))
	flag.Visit(func(f *flag.Flag) {
		seen[f.Name] = true
	})

	for _, name := range names {
		if !seen[name] {
			return false
		}
	}
	return true
}
