# 🍑 PokerAlgo - Texas Hold 'em hand evaluator and simulation engine.
### **PokerAlgo** is a C# library for evaluating poker hands, determining winners, and simulating win probabilities in Texas Hold 'em.
## Features
- Determines the "winning hand" of a given Player.
- Determines the winner or winners in case of a tie.
- Given a winning hand, it will provide a "pretty" name. For example, "Full House, 7s over 5s" or "Pair of 7s".
- Provides the chances of winning or tying a round of Texas Hold 'em.
  - Pre-flop, given a Player's hole cards, it can provide the chances of winning or tying using Monte Carlo simulations or using pre-computed data from a file.
  - Post-flop, it can compute the chances of a player winning or tying using Monte Carlo simulations.
  - Given a Player's hole cards, it can provide a hand strength value using Bill Chen's formula. A value ranging from -1 to 22.
  > Something to note about these predictions is that they are independent of other players. They are only calculated based on what a given player knows, hence why the chances don't add to 100%. The reason I added predictions is so I could build an AI that would make decisions based on its probabilities of winning. 
- Monte Carlo Simulations are parallelized.

## Future Improvements
  - Simulate all players together for chances that add up to 100% on the board.
  - Compute data tables for post-flop hands to avoid using simulations during a live game.

## Compatibility
  - .NET 6+
  - Usable in any C# project

## Installation
```bash
dotnet add package PokerAlgo
```

## How to use
```csharp
using PokerAlgo;

namespace PokerAlgoTest;
class Program
{
    static void Main()
    {
        Deck deck = new();

        List<Player> players = new() // at least 2 players
        {
            new Player("Player 1", deck.NextCard(), deck.NextCard()),
            new Player("Player 2", deck.NextCard(), deck.NextCard()),
            new Player("Player 3", deck.NextCard(), deck.NextCard()),
            new Player("Player 4", deck.NextCard(), deck.NextCard()),
            new Player("Player 5", deck.NextCard(), deck.NextCard()),
        };

        List<Card> community = deck.NextCards(5);

        Console.WriteLine("Players:");
        foreach (var player in players)
        {
            Console.WriteLine(player);
        }

        Console.WriteLine("\nCommunity Cards:");
        foreach (Card item in community)
        {
            Console.Write(item + " ");
        }

        Console.WriteLine();
        Player player1 = players[0];
        HandEvaluator evaluator = new();
        WinningHand winningHand = evaluator.GetWinningHand(player1.HoleCards, community);
        Console.WriteLine("\nPlayer 1 WinningHand: \n" + winningHand);

        Console.WriteLine("\nChances of Player 1 Winning:");
        var chances = ChanceCalculator.GetWinningChanceSimParallel(player1.HoleCards, community, players.Count - 1, 10_000);
        Console.WriteLine($"Win: {chances.winChance}\nTie: {chances.tieChance}");


        List<Player> winners = Algo.GetWinners(players, community);

        Console.WriteLine();
        Console.WriteLine(winners.Count == 1 ? "Winner:" : "Tie:");
        foreach (Player winner in winners)
        {
            Console.WriteLine(winner);
            Console.WriteLine(winner.WinningHand);
            Console.WriteLine(Helpers.GetPrettyHandName(winner.WinningHand));
            Console.WriteLine();
        }

    }
}
```

## License
This project is licensed under the MIT License.

## Licenses Used
- Package Icon based on "OpenMoji" – the open-source emoji set (https://openmoji.org), licensed under CC BY-SA 4.0.
