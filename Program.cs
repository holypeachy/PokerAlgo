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

            List<Player> players = new List<Player>
            {
                new Player("Tom", deck.NextCard(), deck.NextCard()),
                new Player("Matt", deck.NextCard(), deck.NextCard()),
                new Player("Ben", deck.NextCard(), deck.NextCard())
            };

            if(debugEnable){
                foreach (Player p in players)
                {
                    Console.WriteLine(p);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                communityCards.Add(deck.NextCard());
            }

            if (debugEnable)
            {
                Console.Write("\nCommunity Cards:\n\t\t");
                foreach (Card c in communityCards)
                {
                    Console.Write($"{c} ");
                }
                Console.WriteLine();
            }

            Algo.FindWinner(players, communityCards);
            // Testing testing = new();
        }
    }
}

/*
TODO: Combine all methods into the first part of the algo.
TODO: Create tests for first part of algo.
TODO: Determine winning hands in community cards.
TODO: 

? Future Ideas 
? Make PerformFinderTest more modular.
? 

* Changes
* Two Pairs no longer get sorted in reverse.
* Removed low aces before standard flush detection, this would cause false flushes.
* Added SortWinningHands and SortHand methods to player class.
* Added DetermineWinner, BreakTie, and DetermineCommunityHands methods to Algo, yet to be implemented.
*/