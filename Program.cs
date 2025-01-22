using System.Diagnostics;

namespace PokerAlgo
{
	public class Program
	{
		private static int _debuVerbosity = Algo._debugVerbosity;

		static void Main(string[] args)
		{
			int executions = 1;
			int inputVerbosity;
			if(args.Length < 1){ Algo._debugVerbosity = 0; }
			else if(args.Length == 2 && int.TryParse(args[1], out executions))
			{
				int.TryParse(args[0], out inputVerbosity);
				if ((inputVerbosity == 0 || inputVerbosity == 1 || inputVerbosity == 2) && executions < 2)
				{
					Algo._debugVerbosity = inputVerbosity;
				}
			}
			else if (args.Length == 1 && int.TryParse(args[0], out inputVerbosity))
			{
				if (inputVerbosity == 0 || inputVerbosity == 1 || inputVerbosity == 2)
				{
					Algo._debugVerbosity = inputVerbosity;
				}
			}
			else
			{
				throw new Exception("Please Enter Valid Arguments {Number of Executions} {Verbosity Level}");
			}

			Stopwatch watch;
			watch = Stopwatch.StartNew();

			for (int execIndex = 0; execIndex < executions; execIndex++)
			{
				Deck deck = new();
				List<Card> communityCards = new List<Card>();

				List<Player> players = new List<Player>
				{
					new Player("Tom", deck.NextCard(), deck.NextCard()),
					new Player("Matt", deck.NextCard(), deck.NextCard()),
					new Player("Ben", deck.NextCard(), deck.NextCard()),
					// new Player("Sam", deck.NextCard(), deck.NextCard()),
					// new Player("Jim", deck.NextCard(), deck.NextCard()),
				};

				if (_debuVerbosity > 0)
				{
					Console.WriteLine("--- 🚀 Game Starts");
					Console.WriteLine("--- 😎 Players:");
					foreach (Player p in players)
					{
						Console.WriteLine("\t" + p);
					}
				}

				for (int i = 0; i < 5; i++)
				{
					communityCards.Add(deck.NextCard());
				}

				if (_debuVerbosity > 0)
				{
					Console.Write("\n--- 🃏 Community Cards:\n\t\t");
					foreach (Card c in communityCards)
					{
						Console.Write($"{c} ");
					}
					Console.WriteLine();
				}


				// ! Main Code Execution
				// List<Player> winners = Algo.GetWinners(players, communityCards);

				// if (_debuVerbosity > 0)
				// {
				// 	Console.BackgroundColor = ConsoleColor.Blue;
				// 	Console.ForegroundColor = ConsoleColor.Black;
				// 	Console.Write("🥇 Program.Main() Winners:");
				// 	Console.ResetColor();
				// 	Console.WriteLine();
				// 	foreach (Player p in winners)
				// 	{
				// 		Console.BackgroundColor = ConsoleColor.Yellow;
				// 		Console.ForegroundColor = ConsoleColor.Black;
				// 		Console.Write($"\t {p.Name} ");
				// 		Console.BackgroundColor = ConsoleColor.Green;
				// 		Console.Write($" {p.WinningHand.Type} ");
				// 		Console.BackgroundColor = ConsoleColor.Gray;
				// 		Console.Write(string.Join(' ', p.WinningHand.Cards) + " ");
				// 		Console.ResetColor();
				// 		Console.WriteLine();
				// 		Console.WriteLine();
				// 	}
				// }

				// Testing testing = new();

				// ! Monte Carlo Simulation Test
				Player targetPlayer = players[0];
				// Console.WriteLine(targetPlayer);
				// Console.WriteLine("Community Cards: " + string.Join(' ', communityCards));
				// Console.WriteLine($"Winning Hand for Tom: {targetPlayer.Name}");
				HandEvaluator handEvaluator = new();
				List<Card> cards = new()
				{
					targetPlayer.HoleCards.First,
					targetPlayer.HoleCards.Second,
				};
				cards.AddRange(communityCards);
				targetPlayer.WinningHand = handEvaluator.GetWinningHand(cards);
				if(targetPlayer.WinningHand.Type >= HandType.RoyalFlush)
				{
					Console.WriteLine(targetPlayer.WinningHand);
					Console.WriteLine("\tChances of winning: " + (ChanceCalculator.CalculateChances(targetPlayer, communityCards) * 100d) + "%");
				}
				// !
			}

			watch.Stop();
			Console.WriteLine();
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write(executions == 1 ? $" 🕜 Execution Time: {watch.ElapsedMilliseconds}ms " : $" 🕜 Execution Time ({executions} Execs): {watch.ElapsedMilliseconds}ms (Avg {watch.ElapsedMilliseconds/(float)executions}ms)");
			Console.ResetColor();
		}
	}
}

/*
! Current output: Players, Community, Winning Hand for each player.

! ISSUES:
! 

TODO
TODO: The community does not need to be involved at all for winner determination.
TODO: ComparePlayerHands doesn't need players, I can just CompareHands.
TODO: Simulate multiple players in ChanceCalculator.
TODO: Add early break to BreakTies(). "Split" the list to prevent useless loop runs.
TODO: Update tests and create tests for other methods. (Use external dataset instead?)

? Future Ideas 
? Implement custom Exceptions.
? I should make the Algo a nuget package and upload it.
? Full House Logic: The check for Full House could be simplified by directly evaluating the number of threeKinds and pairs. Less branching. if (threeKinds.Count >= 3 && pairs.Count >= 2) { ... }
? Use SortedSet for storing cards when order matters to avoid additional sorting operations.
? Use method extensions for better code readability
? Use Debug.Assert() in spots where I've been throwing errors to assert that something should always be true.
? Should HandEvaluator return a nullable WinningHand object? It should never do so. Use the null-coalescing operator "??".
? ComparePlayerHands doesn't need players, I can just use CompareHands.

* Notes
* "WinningHand nullable? It has been giving me a headache with the warnings." Turns out, it's a good programming pattern.
* 

* Changes
* HandEvaluator class is now responsible for determining the winning hand given a list of cards for either a player or the community.
* I combined the Algo class back into one file since it is not as large.
* Player hand cards have been renamed to HoleCards.
* Card Value has been renamed to Rank.
* Added ChanceCalculator that uses a Monte Carlo Simulation to calculate the chances of a player winning.
* I'll add this here because I can, I realize now that there is no need to find the winning hand in the community; When I designed the original
	* structure of the algorithm I was wrong about how the winner in texas hold em is determined; So I'll be reworking this tomorrow because I'm tired as hell;
	* I don't even know how many hours I've put into this by now. It's been like 2 months.
* 
*/