namespace PokerAlgo
{
    class Program
    {
        private static bool debugEnable = false;

        static void Main()
        {
            Console.Clear();
            Deck deck = new();
            List<Card> communityCards = new List<Card>();

            List<Player> players = new List<Player>();
            players.Add(new Player("Tom", deck.NextCard(), deck.NextCard()));
            players.Add(new Player("Matt", deck.NextCard(), deck.NextCard()));
            players.Add(new Player("Ben", deck.NextCard(), deck.NextCard()));

            if(debugEnable){
                foreach (Player p in players)
                {
                    Console.WriteLine(p);
                }
                Console.Write("\nCommunity Cards:\n");
            }

            for (int i = 0; i < 5; i++)
            {
                communityCards.Add(deck.NextCard());
            }

            if (debugEnable)
            {
                foreach (Card c in communityCards)
                {
                    Console.Write($"{c} ");
                }
                Console.WriteLine();
            }

            // Algo.FindWinner(players, communityCards);
            Testing testing = new();
        }
    }
}

/*
TODO: Replace repeating code with AddWinningHand method.
TODO: Create tests for MultipleFinder.
TODO: Combine all methods into the first part of the algo.
TODO: Create tests for first part of algo.
TODO: Start the second part of the algo, comparing the hands of all players.

? Future Suggestions
? Abstract (verb) Sorting cards, checking for duplicates, and verifying consecutive values.
? Use link in HasConsecutiveValue instead of manual iteration?
? Group cards only once. Then extract fourKind, threeKinds, pairs. 
? Since only StraightFinder needs Aces to be 1 and 14, we can make all aces 14 by default. This simplifies Royal Flush logic.


* Changes
* Replaced winningHand field in TestObject with a listt of winningHands.
* Added a ConvertJson function to Testing to convert the objects in the json tests to another.
* Updated PerformFinderTest to account for multiple possible winning hands in testing; tests pass like normal with new changes.
* Added MultipleFinder method to the Algo to recognize Four of a Kind hands.
* Added log of expected vs actual WinnerHand object information to failed tests.
* Finished MultipleFinder Method.
* PerformFinderTest now checks if suit verification is necessary or not based on the expected winning hand type.
*/