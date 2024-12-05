namespace PokerAlgo
{
    class Program
    {
        private static bool debugEnable = true;

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
                Console.Write("\nCommunity Cards:\n");
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
TODO: Start the second part of the algo, comparing the hands of all players.
TODO: 

? Future Ideas 
? Make PerformFinderTest more modular.
? 

* Changes
* Improved some method names.
* Improved visuals on PerformFinderTest.
* 
*/