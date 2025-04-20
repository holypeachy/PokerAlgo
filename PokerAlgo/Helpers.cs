namespace PokerAlgo;
/// <summary>
/// Contains utility and debug logging methods for inspecting hands, cards, and internal logic during development.
/// </summary>
public static class Helpers
{
#if DEBUG
    private static int _debugVerbosity = 0;
    public static int DebugVerbosity
    {
        get => _debugVerbosity;
        set => _debugVerbosity = (value >= 0 && value <= 2) ? value : 0;
    } // * Verbosity Levels | 0 = Disabled | 1 = Progress Report | 2 = Everything
#else
    internal static int DebugVerbosity { get; set; } = 0;
#endif

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

    /// <summary>
    /// Returns a human-readable string representing the name of a given poker hand (e.g., "Full House, Kings over 3s").
    /// </summary>
    /// <param name="hand">The <see cref="WinningHand"/> you want a pretty name for!</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InternalPokerAlgoException"></exception>
    public static string GetPrettyHandName(WinningHand? hand)
    {
        if (hand is null) throw new ArgumentException("\'hand\' argument must not be null.", nameof(hand));

        return hand.Type switch
        {
            HandType.RoyalFlush => "Royal Flush",

            HandType.StraightFlush => $"{_cardPrintLookUp[hand.Cards[4].Rank]}-High Straight Flush",

            HandType.FourKind => $"Four of a Kind, {_cardPrintLookUp[hand.Cards[4].Rank]}s",

            HandType.FullHouse => $"Full House, {_cardPrintLookUp[hand.Cards[4].Rank]}s over {_cardPrintLookUp[hand.Cards[0].Rank]}s",

            HandType.Flush => $"{_cardPrintLookUp[hand.Cards[4].Rank]}-High Flush",

            HandType.Straight => $"{_cardPrintLookUp[hand.Cards[4].Rank]}-High Straight",

            HandType.ThreeKind => $"Three of a Kind, {_cardPrintLookUp[hand.Cards[4].Rank]}s",

            HandType.TwoPair => $"Two Pair, {_cardPrintLookUp[hand.Cards[4].Rank]}s and {_cardPrintLookUp[hand.Cards[2].Rank]}s",

            HandType.Pair => $"Pair of {_cardPrintLookUp[hand.Cards[4].Rank]}s",

            HandType.Nothing => $"{_cardPrintLookUp[hand.Cards[4].Rank]} High Card",

            _ => throw new InternalPokerAlgoException("Invariant violated: switch defaulted. Was HandType enum changed?"),
        };
    }


#if DEBUG
    public static void DebugLog(string log = "", int verbosity = 1)
    {
        if (DebugVerbosity > verbosity - 1)
        {
            Console.WriteLine(log);
        }
    }

    public static void DebugLogCards(string description, List<Card> cards)
    {
        if (DebugVerbosity > 1)
        {
            Console.WriteLine($"🤖 {description}: ");
            if (cards.Count > 0) Console.Write("\t" + string.Join(' ', cards) + "\n");
            Console.WriteLine();
        }
    }

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
#else
    internal static void DebugLog(string log = "", int verbosity = 1){}

    internal static void DebugLogCards(string description, List<Card> cards){}

    internal static void DebugLogPlayers(string description, List<Player> players) {}

    internal static void DebugLogWinners(List<Player> winners){}

    internal static void DebugLogDeterminingHand(string playerName){}

    internal static void DebugLogWinningHand(HandType handType, List<Card> cards){}
#endif

}