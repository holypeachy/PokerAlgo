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

TODO: Combine all methods into the first part of the algo.
TODO: Create tests for first part of algo.
TODO: Determine winning hands in community cards.
TODO: Start the second part of the algo, comparing the hands of all players.

? Future Suggestions
? Abstract out Sorting cards, checking for duplicates, and verifying consecutive values.
? Use LINQ in HasConsecutiveValue instead of manual iteration?
? Group cards only once. Then extract fourKind, threeKinds, pairs. 
? Since only StraightFinder needs Aces to be 1 and 14, we can make all aces 14 by default. This simplifies Royal Flush logic.
? Abstract out debug logs into its own function?

* Changes
* Added Dictionaries (Look up table) for the ToString method of Card to print A,J,Q,K instead of number ranks.
* The card to string now handles displaying if the card is a player's, this prevents repeating code.
* Added LogCards and SortCardsByValue helper methods to abstract repeated code.
* Added DeterminePlayerHands2, cleaner than the first.
*/