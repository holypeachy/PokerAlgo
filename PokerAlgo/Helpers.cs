using System.Diagnostics;

namespace PokerAlgo;
public static class Helpers
{
    private static int DebugVerbosity => Algo.DebugVerbosity;

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


    public static string GetPrettyHandName(WinningHand? hand)
    {
        if (hand is null) throw new ArgumentException("\'hand\' argument must not be null.", nameof(hand));

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
                throw new InternalPokerAlgoException("Invariant violated: switch defaulted, this should never happen. Was HandType enum changed?");
        }
    }


    [Conditional("DEBUG")]
    public static void DebugLog(string log = "", int verbosity = 1)
    {
        if (DebugVerbosity > verbosity - 1)
        {
            Console.WriteLine(log);
        }
    }

    [Conditional("DEBUG")]
    public static void DebugLogCards(string description, List<Card> cards)
    {
        if (DebugVerbosity > 1)
        {
            Console.WriteLine($"🤖 {description}: ");
            if(cards.Count > 0) Console.Write("\t" + string.Join(' ', cards) + "\n");
            Console.WriteLine();
        }
    }

    [Conditional("DEBUG")]
    public static void DebugLogPlayers(string description, List<Player> players)
    {
        if (DebugVerbosity > 0)
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

    [Conditional("DEBUG")]
    public static void DebugLogWinners(List<Player> winners)
    {
        if (DebugVerbosity > 0)
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("🥇 Winner(s):");
            Console.ResetColor();
            Console.WriteLine();
            foreach (Player p in winners)
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
            }
        }
    }

    [Conditional("DEBUG")]
    public static void DebugLogDeterminingHand(string playerName)
    {
            if (DebugVerbosity > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine("💭 Determining Hand for \'" + playerName + "\'");
                Console.ResetColor();
            }
    }

    [Conditional("DEBUG")]
    public static void DebugLogWinningHand(HandType handType, List<Card> cards)
    {
        if (DebugVerbosity > 0)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write($" {handType}: ");
            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.Write(" ");
            foreach (Card c in cards)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($"{c} ");
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }
}