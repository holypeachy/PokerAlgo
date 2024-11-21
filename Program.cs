namespace PokerAlgo
{
    class Program
    {
        static void Main()
        {
            Deck deck = new();
            List<Card> communityCards = new List<Card>();

            List<Player> players = new List<Player>();
            players.Add(new Player("Tom", deck.NextCard(), deck.NextCard()));
            players.Add(new Player("Matt", deck.NextCard(), deck.NextCard()));
            players.Add(new Player("Ben", deck.NextCard(), deck.NextCard()));

            foreach (Player p in players)
            {
                Console.WriteLine(p);
            }

            Console.Write("\nCommunity Cards:\n");
            for (int i = 0; i < 5; i++)
            {
                communityCards.Add(deck.NextCard());
            }

            foreach (Card c in communityCards)
            {
                Console.Write($"{c} ");
            }

            Algo.FindWinner(ref players, communityCards);
        }
    }

    class Player
    {
        public string Name;
        public Tuple<Card, Card> Hand;


        public Player(string name, Card first, Card second)
        {
            this.Name = name;
            Hand = new Tuple<Card, Card>(first, second);
        }

        public override string ToString()
        {
            return $"{Name}: {Hand.Item1} {Hand.Item2}";
        }
    }
    
}