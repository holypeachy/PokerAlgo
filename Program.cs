﻿namespace PokerAlgo
// TODO: Unit testing (JSON)
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

    class Player
    {
        public string Name { get; set; }
        public Tuple<Card, Card> Hand { get; set; }
        public int HighestScore { get; set; }
        public List<WinningHand> WinningHands { get; set; }


        public Player(string name, Card first, Card second)
        {
            this.Name = name;
            Hand = new Tuple<Card, Card>(first, second);
            Hand.Item1.IsPlayerCard = true;
            Hand.Item2.IsPlayerCard = true;

            HighestScore = 0;
            WinningHands = new();
        }

        public override string ToString()
        {
            return $"{Name}: {Hand.Item1} {Hand.Item2}";
        }
    }

    class WinningHand
    {
        public HandType Type {get; set;}
        public List<Card> Cards { get; set; }

        public WinningHand(HandType type, List<Card> cards)
        {
            this.Type = type;
            this.Cards = cards;
        }
    }

    enum HandType
    {
        Nothing = 0, Pair = 1, TwoPair, ThreeKind, Straight, Flush, FullHouse, FourKind, StraightFlush, RoyalFlush
    }

    enum CardSuit
    {
        Spades, Clubs, Hearts, Diamonds
    }
}