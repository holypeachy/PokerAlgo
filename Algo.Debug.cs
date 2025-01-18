// File: Algo.Debug.cs
namespace PokerAlgo
{
    static partial class Algo
    {
        // * Debug Logging
        private static void DebugLog(string log = "", int verbosity = 1)
        {
            if (_debugVerbosity > verbosity - 1)
            {
                Console.WriteLine(log);
            }
        }

        private static void DebugLogCards(string description, List<Card> cards)
        {
            if (_debugVerbosity > 1)
            {
                Console.WriteLine($"🤖 {description}: ");
                Console.Write("\t" + string.Join(' ', cards) + "\n\n");
            }
        }

        private static void DebugLogPlayers(string description, List<Player> players)
        {
            if (_debugVerbosity > 0)
            {
                Console.WriteLine($"🤖 {description}:");
                foreach (Player p in players)
                {
                    Console.Write($"\t 🃏 Winning Hand ");
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write($" {p.Name} ");
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

        private static void DebugLogPlayers(string description, List<Player> players, Player community)
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