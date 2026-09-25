# PokerAlgo Architecture Flow

_Prepared with Codex._

This is the simple mental model for the project: PokerAlgo is a small poker engine with three main jobs.

1. Represent cards, players, and decks.
2. Evaluate the best hand a player can make.
3. Use that evaluator to either pick winners or estimate chances.

Everything else mostly exists to support those three jobs.

## Big Picture

The project is built around one central idea:

`Evaluate` determines how strong one player's cards are.

`DetermineWinners` uses `Evaluate` for every player, then compares the results.

The chance calculator repeatedly uses `DetermineWinners` inside simulated games.

So the dependency flow is:

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
ChanceCalculator
```

The evaluator is the base of the whole project. If it correctly identifies the best five-card hand, the winner logic and simulations can build on top of it.

## Main Flow: Finding a Winner

For a finished Texas Hold'em hand:

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

The important thing is that `DetermineWinners` does not know how to discover a flush, straight, full house, etc. It delegates that to `Evaluate`. Its job is comparison.

## Hand Evaluation

`Evaluate` takes 5 to 7 cards and returns one `Hand`.

At a high level, it:

1. Validates the cards.
2. Sorts them by rank.
3. Checks for hands from strongest to weakest.
4. Returns the first matching hand.

The order matters:

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

This works because once a stronger hand is found, weaker hands no longer matter.

The returned `Hand` always stores the best five cards. The rest of the code relies on those cards being ordered consistently so ties can be broken later.

## Tie Breaking

After every player has a `BestHand`, `DetermineWinners` compares them.

First it compares the hand type:

```text
Flush beats Straight
Full House beats Flush
Pair loses to Two Pair
```

If two players have the same hand type, it compares the important ranks and kickers.

Examples:

- Pair vs pair: compare the pair rank, then the three kickers.
- Two pair vs two pair: compare top pair, lower pair, then kicker.
- Straight vs straight: compare the highest card.
- Flush vs flush: compare all five cards high-to-low.
- Royal flush vs royal flush: always tied.

This is why the evaluator's five-card output matters so much. The winner code trusts that shape.

## Simulation Flow

The probability code is just winner selection repeated many times.

For a post-flop simulation:

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

Preflop simulation is the same idea, except only the player's two hole cards are known, so the simulation deals all five community cards.

The parallel versions split the number of simulations across CPU cores and add the results together at the end.

## Preflop Lookup

Simulating preflop chances is expensive, so the repo includes generated `.preflop` files.

The lookup path is:

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

The compute command generates those files by running lots of Monte Carlo simulations for every starting hand.

## Deck Mental Model

The deck keeps all 52 cards in memory and tracks the next drawable card with an internal cursor. `Remaining` reports how many cards can still be drawn.

Drawing cards moves the index forward.

Removing known cards for simulation moves those cards into the already-used part of the deck, so future draws cannot hit them.

That means the simulation pattern is always:

```text
reset deck
remove known cards
draw unknown cards
```

## Short Version

PokerAlgo is simple if you think of it in layers:

```text
Deck deals cards.
HandEvaluator picks the best 5-card hand.
Algo compares those hands to find winners.
ChanceCalculator simulates many games by repeatedly calling Algo.
Preflop lookup skips simulation by reading generated results from files.
```

The main invariant to remember is that `Hand` is not just a label. It is the exact five-card hand in the order that the comparison logic expects.
