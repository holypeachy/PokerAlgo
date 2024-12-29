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
			Algo.FindWinner(players, communityCards);
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
? Idea: hasChangesBeenMade bool to keep track of loop, if no we can move on. If yes we need to check one more time.

! TOFIX: Rewrite the second part, it fucking sucks.

TODO: Finish FindWinner method.
TODO: Create tests for everything.
TODO: Separate Algo class into several files.

? Future Ideas 
? Make PerformFinderTest more modular. That or make a new Testing system using Attributes and Reflection.
? Implement custom Exceptions.
? I should make the Algo a nuget package and upload it.
? Full House Logic: The check for Full House could be simplified by directly evaluating the number of threeKinds and pairs. Less branching.
? Use SortedSet for storing cards when order matters to avoid additional sorting operations.
? WinningHand nullable? It has been giving me a headache with the warnings

* Changes
* Added verbosity levels for debugging.
* Added control flow by checking if the Player already has a better hand, if so we skip unecessary code execution.
* Added DetermineCommunityWinningHand method.
* Added FindWinner method but need to implement full logic.
* 
*/