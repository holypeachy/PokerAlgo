using System.Diagnostics;
using PokerAlgo;

namespace Project
{
	public class Program
	{
		private static int _debugVerbosity = 0;
		private static int _numOfCommunityCards = 5;
		private static int _numOfSims = 500;
		private static bool _isTesting = false;
		private static bool _isSim = false;
		private static bool _isPreflop = false;

		static void Main(string[] args)
		{
			int executions = 1;
			int inputVerbosity;
			int inputSims;
			bool inputTestDebug = false;

			if(args.Length == 2 && args[0] == "sim" && int.TryParse(args[1], out inputSims)){
				_isSim = true;
				_numOfSims = inputSims;
			}
			else if(args.Length == 2 && args[0] == "test" && bool.TryParse(args[1], out inputTestDebug)){
				_isTesting = true;
			}
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
			else if(args.Length == 1 && args[0] == "test"){
				_isTesting = true;
			}
			else if(args.Length == 1 && args[0] == "sim"){
				_isSim = true;
			}
			else if(args.Length == 1 && args[0] == "preflop"){
				_isPreflop = true;
			}
			else
			{
				Console.WriteLine("⚠️ PokerAlgo: Please Enter Valid Arguments!\n");
				Console.WriteLine("Examples:");
				Console.WriteLine(" Main code execution.");
				Console.WriteLine("\tdotnet run {debug verbosity : 0|1|2}  |  Runs once.");
				Console.WriteLine("\tdotnet run {debug verbosity : 0|1|2} {executions : int}\n");
				Console.WriteLine(" Run Monte Carlo simulations on all players.");
				Console.WriteLine("\tdotnet run sim  |  Default is 500 simulations.");
				Console.WriteLine("\tdotnet run sim {number of simulations : int}\n");
				Console.WriteLine(" Pre-Flop chances.");
				Console.WriteLine("\tdotnet run preflop\n");
				Console.WriteLine(" Runs all tests.");
				Console.WriteLine("\tdotnet run test  |  No testing debug");
				Console.WriteLine("\tdotnet run test {enable debug : bool}");
				Environment.Exit(1);
			}

			_debugVerbosity = Algo.DebugVerbosity;

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

				if (_debugVerbosity > 0 || _isSim)
				{
					Console.WriteLine("--- 🚀 Game Starts");
					Console.WriteLine("--- 😎 Players:");
					foreach (Player p in players)
					{
						Console.Write("\t" +  string.Format("{0:0}%", ChanceCalculator.GetWinningChancePreFlop(p.HoleCards) * 100));
						Console.WriteLine(" - " + p);
					}
				}

				for (int i = 0; i < _numOfCommunityCards; i++)
				{
					communityCards.Add(deck.NextCard());
				}

				if (_debugVerbosity > 0 || _isSim)
				{
					Console.Write("\n--- 🃏 Community Cards:\n\t\t");
					foreach (Card c in communityCards)
					{
						Console.Write($"{c} ");
					}
					Console.WriteLine();
				}


				// ! Unit Testing
				if(_isTesting)
				{
					new Testing(inputTestDebug);
				}

				// ! Monte Carlo Simulation Test
				else if(_isSim)
				{
					Algo.DebugVerbosity = 0;
					HandEvaluator handEvaluator = new();
					Console.WriteLine();
					Console.WriteLine($"Number of Simulations: {_numOfSims}");
					Console.WriteLine("-----------------------------");
					foreach (Player p in players)
					{
						List<Card> cards = new()
						{
							p.HoleCards.First,
							p.HoleCards.Second,
						};
						cards.AddRange(communityCards);
						p.WinningHand = handEvaluator.GetWinningHand(cards);

						Console.BackgroundColor = ConsoleColor.Yellow;
						Console.ForegroundColor = ConsoleColor.Black;
						Console.Write($"\t {p.Name} ");
						Console.BackgroundColor = ConsoleColor.Green;
						Console.Write($" {Helpers.GetPrettyHandName(p.WinningHand)} ");
						Console.BackgroundColor = ConsoleColor.Gray;
						Console.Write(" " + string.Join(' ', p.WinningHand.Cards) + " ");
						Console.ResetColor();
						Console.WriteLine();
						
						Tuple<double, double> chanceTuple= ChanceCalculator.GetWinningChance(p.HoleCards, communityCards, players.Count - 1, _numOfSims);
						string winPercentage = string.Format("{0:0.00}%", chanceTuple.Item1 * 100);
						string tiePercentage = string.Format("{0:0.00}%", chanceTuple.Item2 * 100);

						Console.Write($"\tWin:");
						Console.ForegroundColor = ConsoleColor.Black;
						Console.BackgroundColor = ConsoleColor.Blue;
						Console.Write($" {winPercentage} ");
						Console.ResetColor();
						Console.WriteLine();

						Console.Write($"\tTie:");
						Console.ForegroundColor = ConsoleColor.Black;
						Console.BackgroundColor = ConsoleColor.Blue;
						Console.Write($" {tiePercentage} ");
						Console.ResetColor();
						Console.WriteLine();
						Console.WriteLine();
					}
				}

				// ! Pre-Flop Chances
				else if(_isPreflop)
				{
					Algo.DebugVerbosity = 0;
					Console.WriteLine($"Pre-Flop ");
					Console.WriteLine("-----------------------------");
					foreach (Player p in players)
					{
						Console.WriteLine(p);
						Console.WriteLine("\tChen: " + ChanceCalculator.GetPreFlopChen(p.HoleCards));
						
						Console.Write("\tWin: ");
						Console.ForegroundColor = ConsoleColor.Black;
						Console.BackgroundColor = ConsoleColor.Blue;
						Console.Write(" " + string.Format("{0:0.00}%", ChanceCalculator.GetWinningChancePreFlop(p.HoleCards) * 100) + " ");
						Console.ResetColor();
						Console.WriteLine();
						
					}
					Console.WriteLine();
					Console.WriteLine("AA\n  Chen: " + ChanceCalculator.GetPreFlopChen(new Pair<Card, Card>(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Diamonds, true))));
					Console.WriteLine("  Win: " + string.Format("{0:0.00}%", ChanceCalculator.GetWinningChancePreFlop(new Pair<Card, Card>(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Diamonds, true))) * 100) + "%");
					Console.WriteLine("KAs\n  Chen: " + ChanceCalculator.GetPreFlopChen(new Pair<Card, Card>(new Card(13, CardSuit.Spades, true), new Card(14, CardSuit.Spades, true))));
					Console.WriteLine("  Win: " + string.Format("{0:0.00}%", ChanceCalculator.GetWinningChancePreFlop(new Pair<Card, Card>(new Card(13, CardSuit.Spades, true), new Card(14, CardSuit.Spades, true))) * 100) + "%");
					Console.WriteLine("27o\n  Chen: " + ChanceCalculator.GetPreFlopChen(new Pair<Card, Card>(new Card(2, CardSuit.Spades, true), new Card(7, CardSuit.Diamonds, true))));
					Console.WriteLine("  Win: " + string.Format("{0:0.00}%", ChanceCalculator.GetWinningChancePreFlop(new Pair<Card, Card>(new Card(2, CardSuit.Spades, true), new Card(7, CardSuit.Diamonds, true))) * 100) + "%");
				}

				// ! Main Code Execution
				else
				{
					List<Player> winners = Algo.GetWinners(players, communityCards);

					if (_debugVerbosity > 0)
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
				}

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
TODO: 

? Future Ideas
? Look into when to use Debug.Assert vs when to throw an Exception.
? Implement custom Exceptions.
? I should make the Algo a nuget package and upload it.
? Use method extensions for better code readability.

? Use SortedSet for storing cards when order matters to avoid additional sorting operations. ??
? Full House Logic: The check for Full House could be simplified by directly evaluating the number of threeKinds and pairs. Less branching. if (threeKinds.Count >= 3 && pairs.Count >= 2) { ... }

* Notes
* "WinningHand nullable? It has been giving me a headache with the warnings." Turns out, it's a good programming pattern.
* Null-coalescing operator "??".

* Changes
* Added AlgoUnitTest class and AlgoUnitTests.json with 14 test cases.
* Fixed a logical bug in Testing class.
* Added more color to the logs and improved readability.
* Added chances of ties to ChanceCalculator, the GetWinningChance function now returns a tuple with win and tie chances.
* 
*/