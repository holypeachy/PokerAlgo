# PokerAlgo Architecture

3 main jobs:

1. Represent cards, players, and decks.
2. Evaluate the best hand a player can make.
3. Use that evaluator to either pick winners or estimate chances.

## Big Picture

```text
Cards / Deck / Player
        |
        v
Evaluate
        |
        v
DetermineWinners
        |
        v
Simulations
```

## Finding Winners

```text
players + 5 community cards
        |
        v
DetermineWinners
        |
        v
for each player:
  combine 2 hole cards + 5 community cards
  ask Evaluate for the best 5-card hand
        |
        v
sort players by hand type
        |
        v
compare tied hand types with kickers
        |
        v
return one winner or multiple tied winners
```

`Evaluate` identifies hands. `DetermineWinners` compares them.

## Hand Evaluation

`Evaluate`: 5-7 cards in, one five-card `Hand` out.

1. Validates the cards.
2. Sorts them by rank.
3. Checks for hands from strongest to weakest.
4. Returns the first matching hand.

Evaluation order:

```text
Royal Flush
Straight Flush
Four of a Kind
Full House
Flush
Straight
Three of a Kind
Two Pair
One Pair
High Card
```

Invariant: `Hand.Cards` contains the best five cards in the order expected by comparison logic.

## Tie Breaking

Compare hand type first:

```text
Flush beats Straight
Full House beats Flush
Pair loses to Two Pair
```

Same type: compare ranks, then kickers.

- Pair vs pair: compare the pair rank, then the three kickers.
- Two pair vs two pair: compare top pair, lower pair, then kicker.
- Straight vs straight: compare the highest card.
- Flush vs flush: compare all five cards high-to-low.
- Royal flush vs royal flush: always tied.

## Simulation Flow

Postflop:

```text
known player cards + known community cards
        |
        v
repeat N times:
  reset deck
  remove known cards
  deal unknown opponent cards
  deal missing community cards
  run DetermineWinners
  count win or tie for the target player
        |
        v
return win chance and tie chance
```

- Preflop: same flow, but all five community cards are dealt.
- Parallelism: one job per logical CPU, one goroutine per job, then combine results.
- Intended live workload: approximately 10,000 simulations per AI decision.
- Optimization TODO: bulk simulation runs allocate up to roughly 30 GB cumulatively, not all resident at once. Reduce heap allocations.

## Preflop Lookup

Precomputed win/tie chances in `.preflop` files:

```text
hole cards
        |
        v
convert to notation like AKs, AKo, 77o
        |
        v
load .preflop files from folder
        |
        v
find matching notation + opponent count
        |
        v
return stored win/tie chance
```

`cmd/compute` generates these files using Monte Carlo simulations for each starting hand.

## Deck

- 52 cards and an internal draw cursor.
- Drawing advances the cursor; `Remaining` reports drawable cards.
- Removing known cards moves them into the used portion of the deck.

Simulation draw order:

```text
reset deck
remove known cards
draw unknown cards
```
