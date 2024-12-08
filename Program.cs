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
            // communityCards.Add(new Card(14, CardSuit.Spades, false));
            // communityCards.Add(new Card(14, CardSuit.Clubs, false));
            // communityCards.Add(new Card(14, CardSuit.Diamonds, false));
            // communityCards.Add(new Card(13, CardSuit.Spades, false));
            // communityCards.Add(new Card(13, CardSuit.Clubs, false));

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
! Since the tie breaker code only runs through the winners once, there is a chance of:
Winners:
        Ben: Type: Pair| Cards: [A,Spades]🙂 [A,Diamonds]
        Matt: Type: Pair| Cards: [K,Hearts]🙂 [K,Diamonds]
        Tom: Type: Pair| Cards: [K,Clubs]🙂 [K,Diamonds]

Pair Tie
Ben: Type: Pair| Cards: [A,Spades]🙂 [A,Diamonds]
Tom: Type: Pair| Cards: [K,Clubs]🙂 [K,Diamonds]
? hasChangesBeenMade if no we can move on. If yes we need to check one more time.
! BUT:
TODO: Rewrite the second part, it fucking sucks

TODO: Combine all methods into the first part of the algo.
TODO: Create tests for first part of algo.
TODO: Determine winning hands in community cards.
TODO: 

? Future Ideas 
? Make PerformFinderTest more modular.
? 

* Changes
* Adjusted some tests in MultipleTests.json; I altered the FindMultiple to make sure pairs are in increasing order.
* Finished second part of algo, need to rewrite it with better planning.
*/