namespace PokerAlgo
{
    class Program
    {
        static void Main()
        {
            Console.Clear();
            Deck deck = new();
            List<Card> communityCards = new List<Card>();

            List<Player> players = new List<Player>();
            players.Add(new Player("Tom", deck.NextCard(), deck.NextCard()));
            players.Add(new Player("Matt", deck.NextCard(), deck.NextCard()));
            players.Add(new Player("Ben", deck.NextCard(), deck.NextCard()));

            // foreach (Player p in players)
            // {
            //     Console.WriteLine(p);
            // }

            // Console.Write("\nCommunity Cards:\n");
            for (int i = 0; i < 5; i++)
            {
                communityCards.Add(deck.NextCard());
            }

            // foreach (Card c in communityCards)
            // {
            //     Console.Write($"{c} ");
            // }
            // Console.WriteLine();

            // Algo.FindWinner(players, communityCards);
            Testing testing = new();
        }
    }
}

/*
TODO: Implement StraightFinder.

* Changes
* Added bool for debug messages and bool for testing.
* Organized the project better by using folders.
*/