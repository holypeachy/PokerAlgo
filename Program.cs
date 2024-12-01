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
* Added a Description property to the TestObject for readability.
* Added all appropiate test cases in FlushTests.json for testing flushes.
* Fixes issue where test would crash the program, because in the case no flush was found the function would return early and not add a
 winning hand object to the player.
* Replaced TestFlushes with a generic function that takes in the path of the test and the method of Algo to test, for reusability.
* Added Equals method to Card class.
* Created files for classes and enums.
*/