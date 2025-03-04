﻿using System.Diagnostics;
using System.Text;
using PokerAlgo;

namespace Project
{
	public class Program
	{
		private static int _debugVerbosity = 0;
		private static int _executions = 1;
		private static int _numOfCommunityCards = 5;
		private static int _numOfSims = 500;
		private static bool _isTesting = false;
		private static bool _inputTestDebug = false;
		private static bool _isSim = false;
		private static bool _isPreflop = false;
		private static bool _isCompute = false;
		private static string _preflopFilePath = "";
		private static int _numOfPreflopSimPlayers = 0;

		private static readonly Dictionary<int, string> _cardPrintLookUp = new Dictionary<int, string>
		{
			{1, "A"}, {2, "2"}, {3, "3"}, {4, "4"}, {5, "5"}, {6, "6"}, {7, "7"}, {8, "8"}, {9, "9"}, {10, "10"},{11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}
		};


		static void Main(string[] args)
		{
			int inputVerbosity;
			int inputSims;

			if (args.Length == 4 && args[0] == "compute" && int.TryParse(args[1], out _numOfPreflopSimPlayers) && int.TryParse(args[2], out inputSims))
			{
				_isCompute = true;
				_preflopFilePath = args[3];
				_numOfSims = inputSims;
			}
			else if (args.Length == 2 && args[0] == "sim" && int.TryParse(args[1], out inputSims))
			{
				_isSim = true;
				_numOfSims = inputSims;
			}
			else if (args.Length == 2 && args[0] == "test" && bool.TryParse(args[1], out _inputTestDebug))
			{
				_isTesting = true;
			}
			else if (args.Length == 2 && args[0] != "compute" && int.TryParse(args[0], out inputVerbosity) && int.TryParse(args[1], out _executions))
			{
				if ((inputVerbosity == 0 || inputVerbosity == 1 || inputVerbosity == 2) && _executions < 2)
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
			else if (args.Length == 1 && args[0] == "test")
			{
				_isTesting = true;
			}
			else if (args.Length == 1 && args[0] == "sim")
			{
				_isSim = true;
			}
			else if (args.Length == 1 && args[0] == "preflop")
			{
				_isPreflop = true;
			}
			else
			{
				Console.WriteLine("⚠️ PokerAlgo: Please Enter Valid Arguments!\n");
				Console.WriteLine("Examples:");
				Console.WriteLine(" Main code execution.");
				Console.WriteLine("\tdotnet run {debug verbosity : 0|1|2}");
				Console.WriteLine("\tdotnet run {debug verbosity : 0|1|2} {executions : int}\n");
				Console.WriteLine(" Run Monte Carlo simulations on all players.");
				Console.WriteLine("\tdotnet run sim");
				Console.WriteLine("\tdotnet run sim {number of simulations : int}\n");
				Console.WriteLine(" Pre-Flop Calculation using Chen's and Custom Sigmoid Equation Normalization.");
				Console.WriteLine("\tdotnet run preflop\n");
				Console.WriteLine(" Runs Pre-Flop Computations.");
				Console.WriteLine("\tdotnet run compute {number of players : int} {number of simulations : int} {output file path : string}\n");
				Console.WriteLine(" Runs all tests.");
				Console.WriteLine("\tdotnet run test");
				Console.WriteLine("\tdotnet run test {enable debug : bool}");
				Environment.Exit(1);
			}

			_debugVerbosity = Algo.DebugVerbosity;
			// _debugVerbosity = 0;

			Stopwatch watch;
			watch = Stopwatch.StartNew();

			for (int execIndex = 0; execIndex < _executions; execIndex++)
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
						Console.Write("\t" + string.Format("{0:0}%", ChanceCalculator.GetWinningChancePreFlop(p.HoleCards) * 100));
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
					Console.WriteLine("\n");
				}


				// ! Unit Testing
				if (_isTesting)
				{
					new Testing(_inputTestDebug);
				}

				// ! Monte Carlo Simulation Test
				else if (_isSim)
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

						Tuple<double, double> chanceTuple = ChanceCalculator.GetWinningChance(p.HoleCards, communityCards, players.Count - 1, _numOfSims);
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
				else if (_isPreflop)
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

				// ! Pre-Flop Computation
				else if (_isCompute)
				{
					if (string.IsNullOrWhiteSpace(_preflopFilePath))
					{
						Console.WriteLine($"⛔ Please enter a valid file path. You entered: \"{_preflopFilePath}\"");
						Environment.Exit(1);
					}

					Console.WriteLine("💭 Computing chances of winning for all starting hands...");

					Dictionary<string, Pair<Card, Card>> startingHands = new();
					Card first;
					Card second;
					for (int i = 2; i <= 14; i++)
					{
						for (int j = 2; j <= 14; j++)
						{
							first = new Card(i, CardSuit.Spades, true);
							second = new Card(j, CardSuit.Hearts, true);
							startingHands[$"{_cardPrintLookUp[first.Rank]} {_cardPrintLookUp[second.Rank]} o"] = new Pair<Card, Card>(first, second);
						}
					}
					for (int i = 2; i <= 14; i++)
					{
						for (int j = 2; j <= 14; j++)
						{
							if (i == j)
							{
								continue;
							}
							first = new Card(i, CardSuit.Hearts, true);
							second = new Card(j, CardSuit.Hearts, true);
							startingHands[$"{_cardPrintLookUp[first.Rank]} {_cardPrintLookUp[second.Rank]} s"] = new Pair<Card, Card>(first, second);
						}
					}

					try
					{
						File.Delete(_preflopFilePath);
					}
					catch
					{
						throw new IOException($"⛔ Issue when trying to delete before calculation \"{_preflopFilePath}\".");
					}

					StringBuilder results = new StringBuilder();
					results.AppendLine($"Pre-Flop Computations | Opponents: {_numOfPreflopSimPlayers} | Simulations: {_numOfSims}\n");
					
					foreach (KeyValuePair<string, Pair<Card, Card>> keyValuePair in startingHands)
					{
						Tuple<double, double> chanceTuple = ChanceCalculator.GetWinningChancePreFlopSim(keyValuePair.Value, _numOfPreflopSimPlayers, _numOfSims);
						results.AppendLine($"{keyValuePair.Key} {chanceTuple.Item1} {chanceTuple.Item2}");
					}

					try
					{
						File.AppendAllText(_preflopFilePath, results.ToString());
					}
					catch
					{
						throw new IOException($"⛔ Issue when writing to \"{_preflopFilePath}\".");
					}

					Console.WriteLine($"✅ Computations done. Data can be found in {_preflopFilePath}");
				}

				// ! Main Code Execution
				else
				{
					List<Player> winners = Algo.GetWinners(players, communityCards);

					if (_debugVerbosity > 0)
					{
						Console.BackgroundColor = ConsoleColor.Blue;
						Console.ForegroundColor = ConsoleColor.Black;
						Console.Write("🥇 Program.Main() Winner(s):");
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
			Console.Write(_executions == 1 ? $" 🕜 Execution Time: {watch.ElapsedMilliseconds}ms " : $" 🕜 Execution Time ({_executions} Execs): {watch.ElapsedMilliseconds}ms (Avg {watch.ElapsedMilliseconds / (float)_executions}ms)");
			Console.ResetColor();
		}
	}
}

/*
! ISSUES:
! 

TODO
TODO: Don't deep copy cards in the ChanceCalculator. Instead just use the hole cards reference.
TODO: Double check last commit's code.
TODO: Implement custom Exceptions.

? Future Ideas
? Debug code for nuget package?! Performance impact / cleaner code.
? I should make the Algo a nuget package and upload it.
? Use method extensions for better code readability.

? Use SortedSet for storing cards when order matters to avoid additional sorting operations. ??
? Full House Logic: The check for Full House could be simplified by directly evaluating the number of threeKinds and pairs. Less branching. if (threeKinds.Count >= 3 && pairs.Count >= 2) { ... }
? Multithreading for Monte Carlo sim.

* Notes
* "WinningHand nullable? It has been giving me a headache with the warnings." Turns out, it's a good programming pattern.
* Null-coalescing operator "??".

* Changes
* Added GetWinningChancePreFlopSim function to ChanceCalculator which runs Monte Carlo simulations for a player's given cards.
* Added compute flag to compute the winning chances of all possible preflop hands and output them to a file.
* 
*/