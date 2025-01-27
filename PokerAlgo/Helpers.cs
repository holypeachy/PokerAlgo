namespace PokerAlgo
{
    internal static class Helpers
    {
        private static int _debugVerbosity = Algo.DebugVerbosity;

        private static readonly Dictionary<int, string> _cardPrintLookUp = new Dictionary<int, string>
        {
            {14, "Ace"},
            {13, "King"},
            {12, "Queen"},
            {11, "Jack"},
            {10, "10"},
            {9, "9"},
            {8, "8"},
            {7, "7"},
            {6, "6"},
            {5, "5"},
            {4, "4"},
            {3, "3"},
            {2, "2"},
            {1, "Ace"},
        };


        public static void DebugLog(string log = "", int verbosity = 1)
        {
            if (_debugVerbosity > verbosity - 1)
            {
                Console.WriteLine(log);
            }
        }

        public static void DebugLogCards(string description, List<Card> cards)
        {
            if (_debugVerbosity > 1)
            {
                Console.WriteLine($"🤖 {description}: ");
                if(cards.Count > 0) Console.Write("\t" + string.Join(' ', cards) + "\n");
                Console.WriteLine();
            }
        }

        public static void DebugLogPlayers(string description, List<Player> players)
        {
            if (_debugVerbosity > 0)
            {
                Console.WriteLine($"🤖 {description}:");
                foreach (Player p in players)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write($"\t {p.Name} ");
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write($" {GetPrettyHandName(p.WinningHand)} ");
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(" " + string.Join(' ', p.WinningHand.Cards) + " ");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

        public static string GetPrettyHandName(WinningHand? hand)
        {
            if (hand is null) throw new Exception("Helpers.GetPrettyName() - \'hand\' argument must not be null.");

            switch (hand.Type) {
                case HandType.RoyalFlush:
                    return "Royal Flush";

                case HandType.StraightFlush:
                    return $"{_cardPrintLookUp[hand.Cards.ElementAt(4).Rank]}-High Straight Flush";

                case HandType.FourKind:
                    return $"Four of a Kind, {_cardPrintLookUp[hand.Cards.ElementAt(4).Rank]}s";

                case HandType.FullHouse:
                    return $"Full House, {_cardPrintLookUp[hand.Cards.ElementAt(4).Rank]}s over {_cardPrintLookUp[hand.Cards.ElementAt(0).Rank]}s";

                case HandType.Flush:
                    return $"{_cardPrintLookUp[hand.Cards.ElementAt(4).Rank]}-High Flush";

                case HandType.Straight:
                    return $"{_cardPrintLookUp[hand.Cards.ElementAt(4).Rank]}-High Straight";

                case HandType.ThreeKind:
                    return $"Three of a Kind, {_cardPrintLookUp[hand.Cards.ElementAt(4).Rank]}s";

                case HandType.TwoPair:
                    return $"Two Pair, {_cardPrintLookUp[hand.Cards.ElementAt(4).Rank]}s and {_cardPrintLookUp[hand.Cards.ElementAt(2).Rank]}s";

                case HandType.Pair:
                    return $"Pair of {_cardPrintLookUp[hand.Cards.ElementAt(4).Rank]}s";

                case HandType.Nothing:
                    return $"{_cardPrintLookUp[hand.Cards.ElementAt(4).Rank]} High Card";

                default:
                    throw new Exception("⛔ Helpers.GetPrettyName(): Switch defaulted.");
            }
        }

    }
}