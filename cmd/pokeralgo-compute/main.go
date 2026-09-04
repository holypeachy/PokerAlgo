package main

import (
	"flag"
	"fmt"
	"os"
	"time"

	"pokeralgo/internal/preflopcompute"
)

func main() {
	opponents := flag.Int("opponents", 0, "maximum number of opponents to compute, starting at 1")
	sims := flag.Int("sims", 0, "number of Monte Carlo simulations per starting hand")
	outDir := flag.String("out", "", "output directory for .preflop files")
	flag.Parse()

	if !allFlagsProvided("opponents", "sims", "out") {
		fmt.Fprintln(os.Stderr, "⚠️ pokeralgo-compute: please provide required flags")
		flag.Usage()
		os.Exit(1)
	}

	started := time.Now()
	fmt.Println("💭 Computing chances of winning for all starting hands...")
	fmt.Printf("Number of Opponents: %d\n", *opponents)
	fmt.Printf("Simulations per Hand: %d\n", *sims)
	fmt.Println("Parallel: true")
	fmt.Printf("Output Directory: %s\n\n", *outDir)

	err := preflopcompute.Run(preflopcompute.Options{
		MaxOpponents: *opponents,
		Sims:         *sims,
		OutDir:       *outDir,
	})
	if err != nil {
		fmt.Fprintf(os.Stderr, "⛔ compute failed: %v\n", err)
		flag.Usage()
		os.Exit(1)
	}

	fmt.Printf("\n🕜 Execution Time: %s\n", time.Since(started).Round(time.Millisecond))
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
