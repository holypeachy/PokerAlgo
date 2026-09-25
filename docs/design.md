# PokerAlgo Technical Design Notes

_Prepared with Codex._

This document is a maintainer memory aid for the Go version of PokerAlgo. It is based on the current repository, the checked-in tests and fixtures, the local git history, and the attached README from the previous C# version. Statements marked as inference are not directly guaranteed by code.

## Project Shape

PokerAlgo is a Go module named `pokeralgo`. The root package exposes the reusable poker logic:

- `card.go`, `types.go`: cards, suits, players, hand types, pairs, and winning-hand data structures.
- `deck.go`: seeded/random deck construction, shuffle, draw, reset, and known-card removal.
- `hand_evaluator.go`: best five-card hand classification from 5-7 cards.
- `algo.go`: showdown winner selection across players.
- `chance_calculator.go`: Monte Carlo equity estimates, preflop Chen score, and preflop lookup entry points.
- `preflop_loader.go`: file-backed preflop lookup table loading.
- `guards.go`, `errors.go`: validation and typed error categories.
- `debug.go`, `helpers.go`: debug output and display names.

There are two command packages:

- `cmd/pokeralgo-sandbox`: manual inspection utility with modes for showdown, simulations, Chen score, lookup data, preflop computation, and fixture templates.
- `cmd/pokeralgo-compute`: focused CLI for generating `.preflop` files through `internal/preflopcompute`.

Repository data:

- `resources/preflop_data`: generated lookup files for 1-4 opponents, currently named like `4_500000.preflop`.
- `testdata`: JSON fixtures for hand evaluation and winner selection.

Historical context from the attached C# README: the Go project is a port of a completed C# package. The C# notes describe the larger goal as a Texas Hold'em game/AI support library, with independent win/tie estimates meant to inform an AI rather than sum to 100% across all players. The current Go README and sandbox TODOs show that this remains true for the Go rewrite.

## Core Data Model

Facts demonstrated by code:

- `Card` is `{Rank int, Suit Suit, IsHoleCard bool}`. Suits are `Spades`, `Clubs`, `Hearts`, `Diamonds`.
- `NewCard` accepts ranks `1..14`, but evaluation/showdown validation rejects rank `1`; callers should represent aces as `14`. Rank `1` is used internally to detect ace-low straights after validation.
- `Card.Equal` compares rank and suit only. `IsHoleCard` is display/selection metadata, not card identity.
- `Player` owns `Name`, two `HoleCards`, and an optional `BestHand`.
- `NewPlayer` and `Player.SetHoleCards` mark both hole cards with `IsHoleCard = true` and clear any previous `BestHand`.
- `Hand.Cards` is expected to contain exactly five cards ordered low-to-high by rank, with kickers before the made-hand cards in the pair/trips/quads cases.

Important invariant:

- Most comparison code depends on the five winning cards being sorted ascending and arranged so the key made-hand ranks are at fixed positions. Examples: quads/trips/pair ranks are read from index `4`, two-pair ranks from indices `2` and `4`, and kickers are compared from high index to low index.

Reasonable inference:

- `IsHoleCard` exists mostly to preserve/read better output during hand construction when duplicate ranks occur. It is not used to determine poker strength once a `Hand` is built.

## Deck Behavior

Facts demonstrated by code and tests:

- `NewDeck` seeds with `time.Now().UnixNano()`. `GenerateSeed` provides a secure game seed, and `NewDeckWithSeed` creates a deterministic deck for games and replay.
- Deck creation uses ranks `2..14` only, so normal decks never contain low ace rank `1`.
- Drawing advances an internal cursor; cards are not physically removed. `Remaining` reports the number of drawable cards.
- `Exclude` finds each card anywhere in the deck. If the card has not already been drawn, it advances the cursor and moves that card into the already-consumed prefix. This preserves a 52-card backing array while ensuring future draws skip known cards.
- `Reset` rewinds the cursor and continues the deck's existing deterministic RNG stream. To restart from a seed, create a new deck with `NewDeckWithSeed`.
- Tests assert uniqueness after reset/remove, empty-deck errors, exact-one-card success, not-enough-card errors, index movement, and seeded reproducibility.

Important usage note from sandbox TODOs and code behavior:

- Monte Carlo paths rely on the pattern `Reset()` followed by `Exclude(knownCards)` before drawing unknown opponents/community cards.

## Hand Evaluation Algorithm

Entry points:

- `Evaluate(combinedCards []Card)`: validates 5-7 unique, non-low-ace cards; clones and sorts them; returns the best five-card `Hand`.
- `EvaluatePlayer(playerHoleCards, communityCards)`: combines 2 hole cards plus community cards, then calls `Evaluate`.

Evaluation flow in `evaluateHand`:

1. Group sorted cards by flush suit, rank count 4, rank count 3, and rank count 2.
2. Check royal flush and straight flush first from suited cards. Ace-low handling is done by adding synthetic rank-1 ace copies only after input validation.
3. Check four of a kind.
4. Check full house from either two trips or one trip plus at least one pair.
5. Check standard flush.
6. Check straight. Duplicate ranks are removed, preferring player cards where possible.
7. Check three of a kind, two pair, one pair.
8. Fall back to high card.

`completeWinningHand` fills a partial made hand to five cards by removing the made cards from all cards, then prepending the highest remaining cards as kickers. This is why comparison code can treat the end of the slice as the made-hand area.

Edge behavior:

- Royal flush is recognized when the best five suited cards start at `10` and are consecutive.
- Ace-low straight/straight flush is represented with a rank-1 ace inside the returned hand, even though rank-1 input is rejected.
- With duplicate ranks in straights, the evaluator tries to retain player cards in the returned five-card set. This affects display/debugging, not winner strength.

Known fragile area:

- The evaluator relies on slice ordering conventions rather than an explicit ranking object. This keeps the code compact, but it means changes to hand construction can silently break winner comparison.

## Winner Selection

Entry point:

- `DetermineWinners(players, communityCards)` requires at least two players and exactly five community cards. It validates uniqueness across all hole and community cards and rejects low ace input.

Execution path:

1. Clone the player slice.
2. For each player, combine their two hole cards with the five community cards and compute `BestHand`.
3. Sort players descending by `BestHand.Type`.
4. Repeatedly compare adjacent candidate winners with `compareWinningHands` until only tied winners remain or a single winner remains.

Tie breaking:

- Different hand types are decided by the `HandType` enum ordering.
- Same hand types use hand-specific rules:
  - `RoyalFlush`: always tie.
  - `StraightFlush`, `Straight`, `Flush`, `HighCard`: compare all five ranks high-to-low.
  - `FourKind`, `ThreeKind`, `OnePair`, `TwoPair`, `FullHouse`: compare made-hand ranks at fixed indices, then kickers where applicable.

Important invariant:

- `compareWinningHands` expects non-nil `Hand` pointers with five sorted cards arranged exactly as the evaluator returns them.

Reasonable inference:

- The iterative `breakTies` structure is inherited from the C# design. It works with the current fixtures, but is more stateful than a direct max-rank fold.

## Probability and Preflop Evaluation

Monte Carlo facts:

- `GetWinningChanceSim` estimates win/tie rates from known hole cards and 3-5 known community cards.
- `GetWinningChancePreFlopSim` estimates preflop win/tie rates from only the player's hole cards.
- Parallel variants split the requested simulation count across `runtime.NumCPU()` goroutines and aggregate wins/ties.
- Each simulation resets a deck, excludes known cards, deals random opponent hands, completes community cards if needed, calls `DetermineWinners`, and counts the named player as win or tie.
- Results are independent from the target player's perspective. They are not a combined table where all known players' chances sum to 100%.

Validation facts:

- Simulation APIs require at least one opponent and at least 100 simulations.
- Post-flop simulation requires 3-5 community cards.
- Duplicate known cards are rejected.
- Unlike showdown/evaluation guards, simulation and Chen guards do not consistently reject rank-1 low ace input before deeper execution. If a rank-1 ace reaches `DetermineWinners` during simulation, it can fail there.

Chen scoring facts:

- `GetPreFlopChen` implements a Bill Chen style score using highest card base value, pair doubling/minimum, suited bonus, gap penalty, connector bonus, and rounding.
- `GetWinningChancePreFlopChen` maps the Chen score through a sigmoid using `handStrengthSensitivity = 0.175` and `baselineWinRate = -1.85`.
- Tests pin known Chen values: `AKs = 12`, `TTo = 10`, `57s = 6`, `27o = -1`, `AAo = 20`.

Preflop lookup facts:

- `FolderLoader.Load` glob-loads `*.preflop` files from a directory, parses opponent count from the filename prefix before `_`, and stores rows keyed by `(notation, opponentCount)`.
- Rows are `notation winChance tieChance`, whitespace-separated.
- Loaded data is cached on the `FolderLoader`; there is no cache invalidation.
- `GetWinningChancePreFlopLookUp` builds notation from the hole-card order plus `s` or `o`.
- Checked-in data contains 325 rows per opponent count for 1-4 opponents.

Preflop compute facts:

- `internal/preflopcompute.BuildStartingHands` generates all ordered off-suit ranks including pairs, then all ordered suited non-pairs.
- `Run` writes one file per opponent count and uses parallel preflop simulation for every generated starting hand.

Notable tradeoff:

- Ordered lookup entries make `AKs` and `KAs` both available and simplify lookup construction, but store symmetric data twice. The sandbox TODO explicitly calls out removing symmetric entries as a future idea.

## Data Flow Summary

Showdown:

`caller -> DetermineWinners -> validate players/community -> per-player Evaluate -> evaluateHand -> sort by HandType -> compareWinningHands -> []Player winners`

Post-flop/turn/river simulation:

`caller -> GetWinningChanceSim/Parallel -> validate known cards -> for each sim: reset deck -> exclude known player/community cards -> deal opponents -> complete board -> DetermineWinners -> count target player win/tie -> Chance`

Preflop simulation:

`caller -> GetWinningChancePreFlopSim/Parallel -> validate hole cards -> for each sim: reset deck -> exclude target hole cards -> deal opponents -> deal full board -> DetermineWinners -> Chance`

Preflop lookup:

`caller -> NewFolderLoader -> GetWinningChancePreFlopLookUp -> loader.Load/cache -> notation+opponent key -> Chance`

Preflop data generation:

`pokeralgo-compute or sandbox compute -> preflopcompute.Run -> BuildStartingHands -> parallel preflop sims -> .preflop files`

## Tests and Edge Cases

Coverage demonstrated by tests:

- Card rank validation and typed error checks.
- Deck boundary behavior, remove/reset invariants, uniqueness, and deterministic seeds.
- `Evaluate` validation for 5-7 cards, duplicates, low ace input, and valid input.
- Fixture coverage for every hand class: royal flush, straight flush including ace-low, quads, full house, flush, straight, trips, two pair, pair, and high card.
- Winner fixture coverage for single winner, two-way/three-way ties, board-made royal/straight/full house/quads, straight vs trips, flush vs full house, pocket aces losing, lowest winning hand, shared pair, and straight flush vs lower flush.
- Chance calculator validation, duplicate hole cards, known Chen scores, lookup known values, lookup symmetry checks, external spot checks, and probability range checks.
- Sequential-vs-parallel simulation equivalence is tested with large sample counts and tolerances.

Current test cost:

- `go test ./...` passed locally in about 28.6 seconds. The slow part is the probabilistic tests using 500,000 and 1,000,000 simulations.

Gaps worth remembering:

- No direct tests for `DescribeHand`.
- No explicit tests for rank-1 inputs flowing into simulation/Chen paths.
- No tests for malformed `.preflop` files beyond an invalid/missing directory case.
- No race/concurrency tests for shared `FolderLoader` use.

## Known Limitations and Unfinished Work

Facts from README/TODOs/code:

- The Go port is active but still being cleaned up after a C#-to-Go rewrite.
- Combined equity for multiple known players is not implemented; current probabilities are independent target-player estimates.
- Post-flop lookup/precomputed tables are not implemented.
- Preflop lookup data has no metadata beyond filename and row values.
- Preflop data currently covers only the checked-in opponent counts; lookup for unsupported counts fails.
- Debug logging is package-global state and mostly aimed at local CLI/manual diagnosis.
- `FolderLoader` caches data but does not protect that cache with synchronization.
- Preflop computation can be expensive because it runs Monte Carlo simulations for every generated starting-hand notation and opponent count.
- The module declares `go 1.26`; that version requirement is higher than many installed Go toolchains.

Reasonable inferences:

- The hand evaluator favors explicit poker-case logic over combinatorial enumeration of every five-card subset. That makes the intended poker reasoning readable, but increases dependence on ordering invariants.
- Current APIs are library-first but still shaped by the original CLI/manual development workflow.
- The generated preflop data appears simulation-derived rather than exact equity.

## Engineering Decisions to Preserve

- Keep public evaluation inputs immutable from the caller's perspective: the evaluator clones and sorts rather than mutating the caller's slice.
- Keep rank `14` as the public ace representation; reserve rank `1` for internal wheel-straight handling.
- Preserve `Hand.Cards` ordering unless all comparison code is updated together.
- Preserve deck exclusion semantics for simulation: excluding known cards should advance the internal cursor only when an excluded card is still in the drawable suffix.
- Preserve typed `PokerAlgoError.Kind` for cases where callers/tests need to distinguish validation failures.
- Preserve deterministic seed support; it is important for reproducible failures and manual debugging.
