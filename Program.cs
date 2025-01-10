using System.Diagnostics;

namespace PokerAlgo
{
	class Program
	{
		private static int _debuVerbosity = Algo._debugVerbosity;

		static void Main()
		{
			var watch = Stopwatch.StartNew();

			Deck deck = new();
			List<Card> communityCards = new List<Card>();

			List<Player> players = new List<Player>
			{
				new Player("Tom", deck.NextCard(), deck.NextCard()),
				new Player("Matt", deck.NextCard(), deck.NextCard()),
				new Player("Ben", deck.NextCard(), deck.NextCard())
			};

			if(_debuVerbosity > 0){
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

			// Code to run
			Algo.DetermineWinner(players, communityCards);
			// Testing testing = new();

			watch.Stop();
			Console.WriteLine();
			var elapsedMs = watch.ElapsedMilliseconds;
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.Write($" 🕜 Execution Time: {elapsedMs}ms ");
			Console.ResetColor();
			Console.WriteLine();
		}
	}
}

/*
* Current dotnet run output: Players, Community, Winning Hand for each player

! ISSUES:
! Since the tie breaker code only runs through the winners once, there is a chance of:
Winners:
		Ben: Type: Pair| Cards: [A,Spades]🙂 [A,Diamonds]
		Matt: Type: Pair| Cards: [K,Hearts]🙂 [K,Diamonds]
		Tom: Type: Pair| Cards: [K,Clubs]🙂 [K,Diamonds]

Pair Tie
Ben: Type: Pair| Cards: [A,Spades]🙂 [A,Diamonds]
Tom: Type: Pair| Cards: [K,Clubs]🙂 [K,Diamonds]

! TOFIX: Rewrite the second part, it fucking sucks.

TODO: Separate Algo class into several files.
TODO: Finish FindWinner method.
TODO: Adjust verbosity levels for second part.
TODO: Create tests for everything.

? Future Ideas 
? Make PerformFinderTest more modular. That or make a new Testing system using Attributes and Reflection. Make testing suite?
? Implement custom Exceptions.
? I should make the Algo a nuget package and upload it.
? Full House Logic: The check for Full House could be simplified by directly evaluating the number of threeKinds and pairs. Less branching.
? Use SortedSet for storing cards when order matters to avoid additional sorting operations.
? Add multiple executions in main method for easier testing.

? WinningHand nullable? It has been giving me a headache with the warnings. NOPE, it's a good programming pattern.

* Changes
* Implemented Community_BreakTieLessFive, which breaks the player tie if the community has a better hand as is less than 5 cards.
* Added PlayerWinningObj class to encapsulate player winning hand cards.
* 
*/