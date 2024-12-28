using System.Diagnostics;

namespace PokerAlgo
{
	class Program
	{
		private static bool _debugEnable = true;

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

			if(_debugEnable){
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

			if (_debugEnable)
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

TODO: Add control flow by checking if the Player already has a better hand, if so we skip unecessary code execution.
TODO: Combine all methods into the first part of the algo. However I wanna do that.
TODO: Write second part of algo, should be easier with the recent changes.
TODO: Create tests for first part of algo.
TODO: Determine winning hands in community cards.
TODO: Separate Algo class into several files.

? Future Ideas 
? Make PerformFinderTest more modular. That or make a new Testing system using Attributes and Reflection.
? Implement custom Exceptions.
? I should make the Algo a nuget package and upload it.

* Changes
* Massive Changes.
* Restructured the algo in the following manner: Player object no longer has a list of WinningHand(s), only one WinningHand is necessary.
* The Winning hand should contain the best five cards, including the winning combo; if no winning hand is found the best 5 cards are picked.
* Since we store the best 5 cards in WinningHand I'll need to rewrite all tests.
* Added Pair class, a mutable tuple for ease.
* Quickly patched up the Testing class so I could run the Algo class.
* Made debug logs look a lot nicer.
* 
*/