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

			
			// ! Main Code Execution
			Algo.DetermineWinner(players, communityCards);
			// Testing testing = new();
			// !
			

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
* Current dotnet run output: Players, Community, Winning Hand for each player.
* SetWinningHand is the last method I checked from the bottom up.

! ISSUES:
! 

TODO: Clean up debug console logs.
TODO: Standardize how to handle winners.
TODO: Remove Testing_AddNoWinningHand.
TODO: Separate Algo class into several files.
TODO: Adjust verbosity levels for second part.
TODO: Update testing class.
TODO: Create tests for everything.

? Future Ideas 
? Make PerformFinderTest more modular. That or make a new Testing system using Attributes and Reflection. Make testing suite?
? Implement custom Exceptions.
? I should make the Algo a nuget package and upload it.
? Full House Logic: The check for Full House could be simplified by directly evaluating the number of threeKinds and pairs. Less branching.
? Use SortedSet for storing cards when order matters to avoid additional sorting operations.
? Add multiple executions in main method for easier testing.
? Use extensions for better code readability.

? WinningHand nullable? It has been giving me a headache with the warnings. NOPE, it's a good programming pattern.

* Changes
* Added ComparePlayerHands method that compares two player hands and determines if one wins or if they both tie.
* Added CompareKickers that simply compares two lists of cards and determines if one wins or tie.
* Removed some depricated methods and commented code.
* 
*/