using System.Diagnostics;
using PokerAlgo;

namespace Project
{
	public class Program
	{
		private static int _debuVerbosity = Algo.DebugVerbosity;
		private static int _numOfCommunityCards = 5;

		static void Main(string[] args)
		{
			int executions = 1;
			int inputVerbosity;
			if(args.Length < 1){ Algo.DebugVerbosity = 0; }
			else if(args.Length == 2 && int.TryParse(args[1], out executions))
			{
				int.TryParse(args[0], out inputVerbosity);
				if ((inputVerbosity == 0 || inputVerbosity == 1 || inputVerbosity == 2) && executions < 2)
				{
					Algo.DebugVerbosity = inputVerbosity;
				}
			}
			else if (args.Length == 1 && int.TryParse(args[0], out inputVerbosity))
			{
				if (inputVerbosity == 0 || inputVerbosity == 1 || inputVerbosity == 2)
				{
					Algo.DebugVerbosity = inputVerbosity;
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
					new Player("Sam", deck.NextCard(), deck.NextCard()),
					new Player("Jim", deck.NextCard(), deck.NextCard()),
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

				for (int i = 0; i < _numOfCommunityCards; i++)
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
				List<Player> winners = Algo.GetWinners(players, communityCards);

				if (_debuVerbosity > 0)
				{
					Console.BackgroundColor = ConsoleColor.Blue;
					Console.ForegroundColor = ConsoleColor.Black;
					Console.Write("🥇 Program.Main() Winners:");
					Console.ResetColor();
					Console.WriteLine();
					foreach (Player p in winners)
					{
						Console.BackgroundColor = ConsoleColor.Yellow;
						Console.ForegroundColor = ConsoleColor.Black;
						Console.Write($"\t {p.Name} ");
						Console.BackgroundColor = ConsoleColor.Green;
						Console.Write($" {Helpers.GetPrettyHandName(p.WinningHand)} ");
						Console.BackgroundColor = ConsoleColor.Gray;
						Console.Write(" " + string.Join(' ', p.WinningHand.Cards) + " ");
						Console.ResetColor();
						Console.WriteLine();
						Console.WriteLine();
					}
				}

				// ! Unit Testing
				// Testing testing = new();

				// ! Monte Carlo Simulation Test
				// Algo.DebugVerbosity = 0;
				// HandEvaluator handEvaluator = new();
				// Console.WriteLine($"Number of Simulations: {500}");
				// Console.WriteLine("-----------------------------");
				// foreach (Player p in players)
				// {
				// 	Player targetPlayer = p;
				// 	List<Card> cards = new()
				// 	{
				// 		targetPlayer.HoleCards.First,
				// 		targetPlayer.HoleCards.Second,
				// 	};
				// 	cards.AddRange(communityCards);
				// 	targetPlayer.WinningHand = handEvaluator.GetWinningHand(cards);
				// 	Console.WriteLine($"Player \'{p.Name}\'");
				// 	Console.WriteLine(targetPlayer.WinningHand);
				// 	string percentage = String.Format("{0:0}", ChanceCalculator.GetWinningChance(targetPlayer.HoleCards, communityCards, players.Count - 1, 500) * 100.0d);
				// 	Console.WriteLine("\tChances of winning: " + percentage + "%\n");
				// }

				// ! END
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
TODO: Start testing.

? Future Ideas 
? Implement custom Exceptions.
? I should make the Algo a nuget package and upload it.
? Use method extensions for better code readability

? Use Debug.Assert() in spots where I've been throwing errors to assert that something should always be true. ??
? Use SortedSet for storing cards when order matters to avoid additional sorting operations. ??
? Full House Logic: The check for Full House could be simplified by directly evaluating the number of threeKinds and pairs. Less branching. if (threeKinds.Count >= 3 && pairs.Count >= 2) { ... }
? Null-coalescing operator "??".

* Notes
* "WinningHand nullable? It has been giving me a headache with the warnings." Turns out, it's a good programming pattern.
* 

* Changes
* Changed file structure based on the files to package.
* Added method GetPrettyHandName().
* 
*/