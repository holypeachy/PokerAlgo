namespace PokerAlgo.Compute;

class Program
{
    private static int _numOfOpponents;
    private static int _numOfSims;

    private static readonly Dictionary<int, string> _cardPrintLookUp = new()
    {
        {1, "A"}, {2, "2"}, {3, "3"}, {4, "4"}, {5, "5"}, {6, "6"}, {7, "7"}, {8, "8"}, {9, "9"}, {10, "T"},{11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}
    };


    static void Main(string[] args)
    {
        if (args.Length != 2 || !int.TryParse(args[0], out _numOfOpponents) || !int.TryParse(args[1], out _numOfSims) || _numOfOpponents < 1 || _numOfSims < 100)
        {
            Console.WriteLine("⚠️ PokerAlgo.Compute: Please enter valid arguments!\n");
            Console.WriteLine("Compute Pre-Flop Data");
            Console.WriteLine("\tdotnet run {opponents : int > 0} {simulations: int >= 100}\n");
            Console.WriteLine("Files are created in the same directory as the executable.");
            Console.WriteLine("Recommended number of simulations is 200,000.");
            Environment.Exit(1);
        }

        Console.Clear();

        Stopwatch watch;
        watch = Stopwatch.StartNew();

        ComputePreFlop();

        watch.Stop();
        Console.WriteLine();
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write($" 🕜 Execution Time: {watch.Elapsed} ");
        Console.ResetColor();
    }

    static void ComputePreFlop()
    {
        Console.WriteLine("💭 Computing chances of winning for all starting hands...");
        Console.WriteLine($"Number of Opponents: {_numOfOpponents}");
        Console.WriteLine($"Simulations per Hand: {_numOfSims}\n");

        Dictionary<string, Pair> startingHands = new();
        Card first;
        Card second;
        for (int i = 2; i <= 14; i++)
        {
            for (int j = 2; j <= 14; j++)
            {
                first = new Card(i, CardSuit.Spades, true);
                second = new Card(j, CardSuit.Hearts, true);
                startingHands[$"{_cardPrintLookUp[first.Rank]}{_cardPrintLookUp[second.Rank]}o"] = new Pair(first, second);
            }
        }
        for (int i = 2; i <= 14; i++)
        {
            for (int j = 2; j <= 14; j++)
            {
                if (i == j)
                {
                    continue;
                }
                first = new Card(i, CardSuit.Hearts, true);
                second = new Card(j, CardSuit.Hearts, true);
                startingHands[$"{_cardPrintLookUp[first.Rank]}{_cardPrintLookUp[second.Rank]}s"] = new Pair(first, second);
            }
        }

        for (int currentOpponents = 1; currentOpponents <= _numOfOpponents; currentOpponents++)
        {
            Console.WriteLine($"Current Number of Opponents: {currentOpponents}");

            string filePath = $"{currentOpponents}_{_numOfSims}.preflop";

            int progress = 0;
            Console.Write($"Progress: {progress++}/{startingHands.Count}");

            StringBuilder results = new();

            foreach (KeyValuePair<string, Pair> keyValuePair in startingHands)
            {
                (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopSim(keyValuePair.Value, currentOpponents, _numOfSims);
                results.AppendLine($"{keyValuePair.Key} {winChance} {tieChance}");

                int currentLineCursor = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor);

                Console.Write($"Progress: {progress++}/{startingHands.Count}");
            }

            Console.WriteLine();

            try
            {
                File.WriteAllText(filePath, results.ToString());
            }
            catch
            {
                throw new IOException($"There was an error when writing to \"{filePath}\".");
            }

            Console.WriteLine($"✅ Computations done. Data can be found in: {filePath}\n");
        }

        Console.WriteLine("🗯️ The data generated can be used by the default PokerAlgo Loader (FolderLoader).");
    }

}