namespace PokerAlgo
{
    internal static class Helpers
    {
        private static int _debugVerbosity = Algo.DebugVerbosity;


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
                    Console.Write($" {p.WinningHand.Type} ");
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(string.Join(' ', p.WinningHand.Cards) + " ");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

        public static void DebugLogPlayers(string description, List<Player> players, Player community)
        {
            if (_debugVerbosity > 0)
            {
                Console.WriteLine($"🤖 {description}:");
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($"\t {community.Name} ");
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write($" {community.WinningHand.Type} ");
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write(string.Join(' ', community.WinningHand.Cards) + " ");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine();
                foreach (Player p in players)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write($"\t {p.Name} ");
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write($" {p.WinningHand.Type} ");
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(string.Join(' ', p.WinningHand.Cards) + " ");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

    }
}