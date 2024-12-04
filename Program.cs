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
TODO: Start the second part of the algo, comparing the hands of all players.

? Future Suggestions
? Abstract (verb) Sorting cards, checking for duplicates, and verifying consecutive values.
? Use link in HasConsecutiveValue instead of manual iteration?
? Group cards only once. Then extract fourKind, threeKinds, pairs. 
? Since only StraightFinder needs Aces to be 1 and 14, we can make all aces 14 by default. This simplifies Royal Flush logic.


* Changes
* Added all appropriate unit tests for MultipleFinder, debugged and all tests pass now.
* Unit tests are a god sent.
* First time I've had to actually use a debugger.
* Fixed annoying new line thing.
* Decks shuffle twice upon creation; makes me feel better.
* Card method EqualsNoSuit has been renamed to EqualsValue and no longer check is card is player card or not.
*/