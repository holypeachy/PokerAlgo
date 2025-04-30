namespace Sandbox;

class Program
{
	private static int _debugVerbosity = 0;
	private static readonly int _numOfCommunityCards = 5;
	private static int _numOfSims = 500;
	private static bool _isTesting = false;
	private static bool _inputTestDebug = false;
	private static bool _isSim = false;
	private static bool _isChenPreflop = false;
	private static bool _isLookupPreflop = false;
	private static bool _isCompute = false;
	private static bool _isManual = false;
	private static string _preflopFolderPath = "";
	private static int _numOfPreflopSimPlayers = 0;

	private static readonly string _pathToPreFlopDirectory = @"C:/Users/Frank/Code/PokerAlgo/Resources/preflop_data/";

	private static readonly Dictionary<int, string> _cardPrintLookUp = new()
	{
		{1, "A"}, {2, "2"}, {3, "3"}, {4, "4"}, {5, "5"}, {6, "6"}, {7, "7"}, {8, "8"}, {9, "9"}, {10, "T"},{11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}
	};

	private static Deck deck = new();
	private static List<Player> players = new();
	private static List<Card> communityCards = new();

	static void Main(string[] args)
	{
		int inputVerbosity;
		int inputSims;

		if (args.Length == 4 && args[0] == "compute" && int.TryParse(args[1], out _numOfPreflopSimPlayers) && int.TryParse(args[2], out inputSims))
		{
			_isCompute = true;
			_preflopFolderPath = args[3];
			_numOfSims = inputSims;
		}
		else if (args.Length == 2 && args[0] == "sim" && int.TryParse(args[1], out inputSims))
		{
			_isSim = true;
			_numOfSims = inputSims;
		}
		else if (args.Length == 2 && args[0] == "test" && bool.TryParse(args[1], out _inputTestDebug))
		{
			_isTesting = true;
		}
		else if (args.Length == 1 && int.TryParse(args[0], out inputVerbosity))
		{
			if (inputVerbosity == 0 || inputVerbosity == 1 || inputVerbosity == 2)
			{
				Helpers.DebugVerbosity = inputVerbosity;
			}
		}
		else if (args.Length == 1 && args[0] == "test")
		{
			_isTesting = true;
		}
		else if (args.Length == 1 && args[0] == "sim")
		{
			_isSim = true;
		}
		else if (args.Length == 1 && args[0] == "chen")
		{
			_isChenPreflop = true;
		}
		else if (args.Length == 1 && args[0] == "lookup")
		{
			_isLookupPreflop = true;
		}
		else if (args.Length == 1 && args[0] == "manual")
		{
			_isManual = true;
		}
		else
		{
			Console.WriteLine("⚠️ PokerAlgo.Sandbox: Please enter valid arguments!\n");
			Console.WriteLine("Examples:");
			Console.WriteLine(" Main code execution.");
			Console.WriteLine("\tdotnet run {debug verbosity : 0|1|2}\n");
			Console.WriteLine(" Manual run.");
			Console.WriteLine("\tdotnet run manual\n");
			Console.WriteLine(" Run Monte Carlo simulations on all players.");
			Console.WriteLine("\tdotnet run sim");
			Console.WriteLine("\tdotnet run sim {number of simulations : int}\n");
			Console.WriteLine(" Pre-flop calculation using pre-computed values.");
			Console.WriteLine("\tdotnet run lookup\n");
			Console.WriteLine(" Pre-flop calculation using Chen's and Custom Sigmoid Equation Normalization.");
			Console.WriteLine("\tdotnet run chen\n");
			Console.WriteLine(" Computes pre-flop winning chances for all hands using Monte Carlo simulations.");
			Console.WriteLine("\tdotnet run compute {number of opponents : int} {number of simulations : int} {output directory : string}\n");
			Console.WriteLine(" Runs logic tests.");
			Console.WriteLine("\tdotnet run test");
			Console.WriteLine("\tdotnet run test {enable debug : bool}\n");
			Environment.Exit(1);
		}

		_debugVerbosity = Helpers.DebugVerbosity;
		// _debugVerbosity = 0;

		Stopwatch watch;
		watch = Stopwatch.StartNew();

		players = new List<Player>
		{
			new("Tom", deck.NextCard(), deck.NextCard()),
			new("Matt", deck.NextCard(), deck.NextCard()),
			new("Ben", deck.NextCard(), deck.NextCard()),
			new("Sam", deck.NextCard(), deck.NextCard()),
			new("Jim", deck.NextCard(), deck.NextCard()),
		};

		if (_debugVerbosity > 0 || _isSim)
		{
			Console.WriteLine("--- 🚀 Game Starts");
			Console.WriteLine("--- 😎 Players:");
			FolderLoader folderLoader = new(_pathToPreFlopDirectory);
			foreach (Player p in players)
			{
				Console.Write("\t" + string.Format("{0:0.00}%", ChanceCalculator.GetWinningChancePreFlopLookUp(p.HoleCards, players.Count - 1, folderLoader).winChance * 100));
				Console.WriteLine(" - " + p);
			}
		}

		for (int i = 0; i < _numOfCommunityCards; i++)
		{
			communityCards.Add(deck.NextCard());
		}

		if (_debugVerbosity > 0 || _isSim)
		{
			Console.Write("\n--- 🃏 Community Cards:\n\t\t");
			foreach (Card c in communityCards)
			{
				Console.Write($"{c} ");
			}
			Console.WriteLine("\n");
		}


		if (_isTesting) LogicTesting();

		else if (_isSim) MonteCarloSim();

		else if (_isChenPreflop) ChenPreFlopChances();

		else if (_isCompute) PreFlopComputation();

		else if (_isLookupPreflop) LookUpPreFlopChances();

		else if (_isManual) Manual();

		else MainExecution();

		watch.Stop();
		Console.WriteLine();
		Console.BackgroundColor = ConsoleColor.Blue;
		Console.ForegroundColor = ConsoleColor.Black;
		Console.Write($" 🕜 Execution Time: {watch.ElapsedMilliseconds}ms ");
		Console.ResetColor();
	}

	static void LogicTesting()
	{
		new LogicTests(_inputTestDebug);
	}

	static void MonteCarloSim()
	{
		Helpers.DebugVerbosity = 0;
		HandEvaluator handEvaluator = new();
		Console.WriteLine();
		Console.WriteLine($"Number of Simulations: {_numOfSims}");
		Console.WriteLine("-----------------------------");
		foreach (Player p in players)
		{
			List<Card> cards = new()
					{
						p.HoleCards.First,
						p.HoleCards.Second,
					};
			cards.AddRange(communityCards);
			p.WinningHand = handEvaluator.GetWinningHand(cards);

			Console.BackgroundColor = ConsoleColor.Yellow;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write($"\t {p.Name} ");
			Console.BackgroundColor = ConsoleColor.Green;
			Console.Write($" {Helpers.GetPrettyHandName(p.WinningHand)} ");
			Console.BackgroundColor = ConsoleColor.Gray;
			Console.Write(" " + string.Join(' ', p.WinningHand.Cards) + " ");
			Console.ResetColor();
			Console.WriteLine();

			(double winChance, double tieChance) chanceTuple = ChanceCalculator.GetWinningChanceSim(p.HoleCards, communityCards, players.Count - 1, _numOfSims);
			string winPercentage = string.Format("{0:0.00}%", chanceTuple.winChance * 100);
			string tiePercentage = string.Format("{0:0.00}%", chanceTuple.tieChance * 100);

			Console.Write($"\tWin:");
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.Write($" {winPercentage} ");
			Console.ResetColor();
			Console.WriteLine();

			Console.Write($"\tTie:");
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.Write($" {tiePercentage} ");
			Console.ResetColor();
			Console.WriteLine();
			Console.WriteLine();
		}
	}

	static void ChenPreFlopChances()
	{
		Helpers.DebugVerbosity = 0;
		Console.WriteLine($"Chen + Sigmoid Pre-Flop");
		Console.WriteLine("-----------------------------");
		foreach (Player p in players)
		{
			Console.WriteLine(p);
			Console.WriteLine("\tChen: " + ChanceCalculator.GetPreFlopChen(p.HoleCards));

			Console.Write("\tWin: ");
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.Write(" " + string.Format("{0:0.00}%", ChanceCalculator.GetWinningChancePreFlopChen(p.HoleCards) * 100) + " ");
			Console.ResetColor();
			Console.WriteLine();

		}
		Console.WriteLine();
		Console.WriteLine("AAo\n  Chen: " + ChanceCalculator.GetPreFlopChen(new Pair(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Diamonds, true))));
		Console.WriteLine("  Win: " + string.Format("{0:0.00}%", ChanceCalculator.GetWinningChancePreFlopChen(new Pair(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Diamonds, true))) * 100) + "%");
		Console.WriteLine("KAs\n  Chen: " + ChanceCalculator.GetPreFlopChen(new Pair(new Card(13, CardSuit.Spades, true), new Card(14, CardSuit.Spades, true))));
		Console.WriteLine("  Win: " + string.Format("{0:0.00}%", ChanceCalculator.GetWinningChancePreFlopChen(new Pair(new Card(13, CardSuit.Spades, true), new Card(14, CardSuit.Spades, true))) * 100) + "%");
		Console.WriteLine("27o\n  Chen: " + ChanceCalculator.GetPreFlopChen(new Pair(new Card(2, CardSuit.Spades, true), new Card(7, CardSuit.Diamonds, true))));
		Console.WriteLine("  Win: " + string.Format("{0:0.00}%", ChanceCalculator.GetWinningChancePreFlopChen(new Pair(new Card(2, CardSuit.Spades, true), new Card(7, CardSuit.Diamonds, true))) * 100) + "%");
	}

	static void LookUpPreFlopChances()
	{
		Helpers.DebugVerbosity = 0;
		FolderLoader folderLoader = new(_pathToPreFlopDirectory);

		Console.WriteLine($"Lookup Pre-Flop Chances");
		Console.WriteLine("-----------------------------");
		foreach (Player p in players)
		{
			(double winChance, double tieChance) chances = ChanceCalculator.GetWinningChancePreFlopLookUp(p.HoleCards, players.Count - 1, folderLoader);

			Console.WriteLine(p);
			Console.Write("\tWin: ");
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.Write(" " + string.Format("{0:0.00}%", chances.winChance * 100) + " ");
			Console.ResetColor();
			Console.WriteLine();

			Console.Write("\tTie: ");
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.Write(" " + string.Format("{0:0.00}%", chances.tieChance * 100) + " ");
			Console.ResetColor();
			Console.WriteLine();

		}
		Console.WriteLine();
	}

	static void PreFlopComputation()
	{
		if (string.IsNullOrWhiteSpace(_preflopFolderPath))
		{
			Console.WriteLine($"⛔ Please enter a valid directory path. You entered: \"{_preflopFolderPath}\"");
			Environment.Exit(1);
		}

		Console.WriteLine("💭 Computing chances of winning for all starting hands...");
		Console.WriteLine($"Simulations per Hand: {_numOfSims}");

		string filePath = _preflopFolderPath + (_preflopFolderPath.Last() == '/' ? "" : '/') + $"{_numOfPreflopSimPlayers}_{_numOfSims}.preflop";

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

		int progress = 0;
		Console.Write($"Progress: {progress++}/{startingHands.Count}");

		StringBuilder results = new StringBuilder();

		foreach (KeyValuePair<string, Pair> keyValuePair in startingHands)
		{
			(double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopSim(keyValuePair.Value, _numOfPreflopSimPlayers, _numOfSims);
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
			throw new IOException($"⛔ There was an error when writing to \"{filePath}\".");
		}

		Console.WriteLine($"✅ Computations done. Data can be found in: {filePath}");
	}

	static void MainExecution()
	{
		List<Player> winners = Algo.GetWinners(players, communityCards);

		if (_debugVerbosity > 0)
		{
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write("🥇 Program.Main() Winner(s):");
			Console.ResetColor();
			Console.WriteLine();
			foreach (Player p in winners)
			{
				Console.BackgroundColor = ConsoleColor.Yellow;
				Console.ForegroundColor = ConsoleColor.Black;
				Console.Write($"\t {p.Name} ");
				Console.BackgroundColor = ConsoleColor.Green;
				Console.Write($" {Helpers.GetPrettyHandName(p.WinningHand)} ");
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.Write(" " + string.Join(' ', p.WinningHand.Cards) + " ");
				Console.ResetColor();
				Console.WriteLine();
				Console.WriteLine();
			}
		}
	}

	static void Manual()
	{
		Stopwatch timer = new();

		Console.WriteLine(players[0]);
		Console.WriteLine();
		foreach (var item in communityCards)
		{
			Console.Write(item + " ");
		}
		Console.WriteLine();

		HandEvaluator evaluator = new();
		List<Card> cards = new();
		cards.AddRange(communityCards);
		cards.Add(players[0].HoleCards.First);
		cards.Add(players[0].HoleCards.Second);
		Console.WriteLine();
		Console.WriteLine(evaluator.GetWinningHand(cards));
		Console.WriteLine();

		Console.WriteLine("4 Opponents, 1 Million Sims\n");
		timer.Start();
		(double win, double tie) result = ChanceCalculator.GetWinningChanceSim(players[0].HoleCards, communityCards, 4, 1_000_000);
		timer.Stop();
		Console.WriteLine($"Single-Thread:\n\twin: {result.win}\n\ttie: {result.tie}\n\ttime: {timer.ElapsedMilliseconds}ms");
		timer.Restart();
		result = ChanceCalculator.GetWinningChanceSimParallel(players[0].HoleCards, communityCards, 4, 1_000_000);
		timer.Stop();
		Console.WriteLine($"Multi-Thread:\n\twin: {result.win}\n\ttie: {result.tie}\n\ttime: {timer.ElapsedMilliseconds}ms");

		Console.WriteLine("\nPre-Flop");
		Console.WriteLine("4 Opponents, 1 Million Sims\n");
		timer.Start();
		result = ChanceCalculator.GetWinningChancePreFlopSimParallel(players[0].HoleCards, 4, 1_000_000);
		timer.Stop();
		Console.WriteLine($"Single-Thread:\n\twin: {result.win}\n\ttie: {result.tie}\n\ttime: {timer.ElapsedMilliseconds}ms");
		timer.Restart();
		result = ChanceCalculator.GetWinningChancePreFlopSimParallel(players[0].HoleCards, 4, 1_000_000);
		timer.Stop();
		Console.WriteLine($"Multi-Thread:\n\twin: {result.win}\n\ttie: {result.tie}\n\ttime: {timer.ElapsedMilliseconds}ms");
	}

	static void Manual2()
	{
		Console.WriteLine(players[0]);
		Console.WriteLine();
		foreach (var item in communityCards)
		{
			Console.Write(item + " ");
		}
		Console.WriteLine();

		HandEvaluator evaluator = new();
		List<Card> cards = new();
		cards.AddRange(communityCards);
		cards.Add(players[0].HoleCards.First);
		cards.Add(players[0].HoleCards.Second);
		Console.WriteLine();
		Console.WriteLine(evaluator.GetWinningHand(cards));
		Console.WriteLine();
		Console.WriteLine("Sim: 500");
		Stopwatch timer = new();
		timer.Start();
		Console.WriteLine(ChanceCalculator.GetWinningChanceSim(players[0].HoleCards, communityCards, 4, 500));
		timer.Stop();
		Console.WriteLine(timer.ElapsedMilliseconds);
		Console.WriteLine();

		Console.WriteLine("Sim: 1,000");
		timer.Restart();
		Console.WriteLine(ChanceCalculator.GetWinningChanceSim(players[0].HoleCards, communityCards, 4, 1000));
		timer.Stop();
		Console.WriteLine(timer.ElapsedMilliseconds);
		Console.WriteLine();

		Console.WriteLine("Sim: 5,000");
		timer.Restart();
		Console.WriteLine(ChanceCalculator.GetWinningChanceSim(players[0].HoleCards, communityCards, 4, 5000));
		timer.Stop();
		Console.WriteLine(timer.ElapsedMilliseconds);
		Console.WriteLine();

		Console.WriteLine("Sim: 10,000");
		timer.Restart();
		Console.WriteLine(ChanceCalculator.GetWinningChanceSim(players[0].HoleCards, communityCards, 4, 10000));
		timer.Stop();
		Console.WriteLine(timer.ElapsedMilliseconds);
		Console.WriteLine();

		Console.WriteLine("Sim: 100,000");
		timer.Restart();
		Console.WriteLine(ChanceCalculator.GetWinningChanceSim(players[0].HoleCards, communityCards, 4, 100000));
		timer.Stop();
		Console.WriteLine(timer.ElapsedMilliseconds);
		Console.WriteLine();

		Console.WriteLine("Sim: 500,000");
		timer.Restart();
		Console.WriteLine(ChanceCalculator.GetWinningChanceSim(players[0].HoleCards, communityCards, 4, 500000));
		timer.Stop();
		Console.WriteLine(timer.ElapsedMilliseconds);
		Console.WriteLine();

		Console.WriteLine("Sim: 1,000,000");
		timer.Restart();
		Console.WriteLine(ChanceCalculator.GetWinningChanceSim(players[0].HoleCards, communityCards, 4, 1000000));
		timer.Stop();
		Console.WriteLine(timer.ElapsedMilliseconds);
	}

}

/*
! ISSUES:
! 

TODO
TODO: Add XML comments to export once, that way I don't have to keep the inline comments always.

? Future Ideas
? Use NewHand method for simulations.
? Semantic Debug Levels. Use an enum for verbosity levels.
? Remove json serialization stuff from Card class, only needed because of logic tests.
? Add SerializableAttribute to all Exception classes? uses? or remove from PokerAlgoException. 
? Only instantiate one Random class per Deck class, instead of creating one everytime we shuffle.
? In ChanceCalculator, use a copy the constructor of List class instead of ToList() for slightly better performance.
? Generate a ton of data on the Monte Carlo sims and find how many simulations give the most accurate prediction while minimizing compute time.
? Make GetWinningChanceSim and GetWinningChancePreFlopSim an overloaded method? Since in GetWinningChanceSim the community cards are already variable.
? Remove duplicate entries on the preflop computation logic. AKo == KAo

? Instead of using tuples, use a record ? This object would hold winning chance and tie chance, it would also make future extensions easier to implement.
? Better IO handling: FolderLoader rejecting badly formatted lines and badly formatted file names.

? Precompute all chances of winning?

* Notes
* "WinningHand nullable? It has been giving me a headache with the warnings." Turns out, it's a good programming pattern.
* Null-coalescing operator "??".
* ResetDeck() AND RemoveCards() together, always before using NextCard().

* Changes
* refactor: add NewHand method to Player class.
* details: made some tiny changes to a few PokerAlgo classes.
*/