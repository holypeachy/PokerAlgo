# PokerAlgo

A Texas Hold'em hand evaluator and probability engine written in Go.

**Status:** Active development, but slow  
**Built with:** C# => Go

## Overview

PokerAlgo is the evaluation and probability layer of my Texas Hold'em project. I wanted to do everything from scratch so this was the first thing that was necessary. It grew from hand classification and winner selection into simulation, starting-hand evaluation, and precomputed preflop data. I would love to precompute all possible hands but that's a pretty bit endeavor, but for my purposes Monte Carlo sims will do for now.

Given a player's hole cards and the community cards, PokerAlgo can determine the best five-card hand, resolve winners and ties, and estimate win and tie rates from the information available to that player.

The current implementation is a Go rewrite of the original C# library. The completed C# version remains available as a [NuGet package](https://www.nuget.org/packages/PokerAlgo). I should mark that as deprecated soon, I don't plan on working on the C# version anymore. I was inspired by Bun's rewrite to Rust using AI, it saved me time but it is still costing me engineering time and energy because it's full of little things I would do differently.

## Features

- Evaluates Texas Hold'em hands and identifies each player's best five cards.
- Resolves winners and ties using hand-specific comparisons and kickers.
- Estimates win and tie rates through sequential or CPU-parallel Monte Carlo simulations.
- Supports pre-flop evaluation through Bill Chen's formula and generated lookup tables.
- Includes command-line tools for manual testing, simulation, and pre-flop data generation.

## Getting Started

PokerAlgo requires Go 1.26 or later.

```sh
git clone https://github.com/holypeachy/PokerAlgo.git
cd PokerAlgo
go test ./...
go run ./cmd/pokeralgo-sandbox -mode main -preflop-dir ./resources/preflop_data
```
> A little note: 1 or 2 tests may fail because some of these are probabilistic, try to rerun it if you want. But 1 test failing doesn't necessarily mean there's a bug.

The sandbox has different modes for inspecting hand evaluation, comparing probability methods, running simulations, and generating test fixture templates. This utility is what I use to help with development.

```sh
go run ./cmd/pokeralgo-sandbox -h
```

## Usage

The core API can be used to evaluate a completed round:

```go
deck := pokeralgo.NewDeck()

players := []pokeralgo.Player{
	pokeralgo.NewPlayer("Alice", deck.MustNextCard(), deck.MustNextCard()),
	pokeralgo.NewPlayer("Bob", deck.MustNextCard(), deck.MustNextCard()),
}

community := deck.MustNextCards(5)

winners, err := pokeralgo.GetWinners(players, community)
if err != nil {
	return err
}

for _, winner := range winners {
	fmt.Printf("%s: %s\n", winner.Name, winner.WinningHand)
}
```

Win and tie rates can be estimated at any point after the hole cards (a player's 2 cards) are known. This example estimates Alice's chances after the flop:

```go
chance, err := pokeralgo.GetWinningChanceSimParallel(players[0].HoleCards,
	community[:3],
	len(players)-1,
	100_000,
)
if err != nil {
	return err
}

fmt.Printf("Win: %.2f%%\n", chance.WinChance*100)
fmt.Printf("Tie: %.2f%%\n", chance.TieChance*100)
```

## Planned Work

- First I need to get it to where I want, because Codex did whatever it felt like doing when porting this.
- Add combined equity calculations for multiple known players from a single simulation (aka all player probabilities add to 100%).
- Revisit post-flop lookup data if live simulation becomes a meaningful bottleneck.
