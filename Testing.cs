using System.Diagnostics;
using System.Text.Json;
using PokerAlgo;

namespace Project
{
	class Testing {
		private static bool _debugEnable = false;


		public Testing(bool debugEnable = false) {
			_debugEnable = debugEnable;
			TestHandEvaluator();
			TestAlgo();
		}

		private static void TestHandEvaluator(string pathToTest = @"./Tests/HandEvalUnitTests.json")
		{
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write($" 🧪 ---Hand Evaluator Tests--- ");
			Console.ResetColor();
			Console.WriteLine();
			Console.WriteLine("-------------------------------------------");

			string json = File.ReadAllText(pathToTest);
			HandEvalUnitTest[]? testObjects = JsonSerializer.Deserialize<HandEvalUnitTest[]>(json);

			if (testObjects is null)
			{
				throw new Exception($"testObjects array is null. is {pathToTest} is empty?");
			}

			int testCount = 1;
			bool passed;
			foreach (HandEvalUnitTest test in testObjects)
			{
				passed = true;
				Console.WriteLine($"Running Test {testCount++}: {test.Description}");
				if (_debugEnable)
				{
					Console.WriteLine(test);
				}

				Player player = new("TestPlayer", test.PlayerCards.First, test.PlayerCards.Second);

				List<Card> cards = new()
				{
					player.HoleCards.First,
					player.HoleCards.Second
				};
				cards.AddRange(test.CommunityCards);

				SortCardsByValue(cards);

				HandEvaluator handEvaluator = new();

				WinningHand? expectedHand = test.ExpectedHand;
				WinningHand? actualHand = handEvaluator.GetWinningHand(cards);

				Debug.Assert(expectedHand is not null, "expectedHand is not null");
				Debug.Assert(actualHand is not null, "actualHand is not null");

				if (expectedHand.Type != actualHand.Type) passed = false;
				else if (!IsListOfCardsEqual(expectedHand.Cards, actualHand.Cards)) passed = false;

				if (passed)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("TEST PASSED ✅");
					Console.ResetColor();
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"TEST FAILED ❌");
					Console.WriteLine($"\tExpected: {expectedHand.Type} " + $"Cards: {string.Join(' ', expectedHand.Cards)}");
					Console.WriteLine($"\tActual: {actualHand.Type} " + $"Cards: {string.Join(' ', actualHand.Cards)}");
					Console.ResetColor();
				}
				Console.WriteLine("-------------------------------------------");
			}
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write($" ↪️ End of Hand Evaluator Tests ");
			Console.ResetColor();
			Console.WriteLine("\n");
		}

		private static void TestAlgo(string pathToTest = @"./Tests/AlgoTests.json")
		{
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write($" 🧪 ---Algo Tests--- ");
			Console.ResetColor();
			Console.WriteLine();
			Console.WriteLine("-------------------------------------------");

			string json = File.ReadAllText(pathToTest);
			AlgoUnitTest[]? testObjects = JsonSerializer.Deserialize<AlgoUnitTest[]>(json);

			if (testObjects is null)
			{
				throw new Exception($"testObjects array is null. is {pathToTest} is empty?");
			}

			int testCount = 1;
			bool passed;
			foreach (AlgoUnitTest test in testObjects)
			{
				passed = true;
				Console.WriteLine($"Running Test {testCount++}: {test.Description}");
				if (_debugEnable)
				{
					Console.WriteLine(test);
				}

				// TODO: Implement Tests
				List<Player> players = new List<Player>
				{
					new Player("Test Player 1", test.Player1.First, test.Player1.Second),
					new Player("Test Player 2", test.Player2.First, test.Player2.Second),
					new Player("Test Player 3", test.Player3.First, test.Player3.Second)
				};
				List<Player> winners = Algo.GetWinners(players, test.CommunityCards);

				Debug.Assert(winners.Count > 0, "winners.Count should always be greater than 0");

				if (winners.Count != test.IndicesOfWinners.Length) passed = false;
				else
				{
					foreach (int index in test.IndicesOfWinners)
					{
						if(!winners.Contains( players.ElementAt(index) ) ) passed = false;
					}
				}

				if (passed)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("TEST PASSED ✅");
					Console.ResetColor();
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"TEST FAILED ❌");
					Console.WriteLine($"\tExpected Winner(s): {string.Join(", ", test.IndicesOfWinners)}");
					string actualIndices = "";
					foreach (Player winner in winners)
					{
						actualIndices += $"{players.IndexOf(winner)}";
					}
					Console.WriteLine($"\tActual Winner(s): {actualIndices} ");
					Console.ResetColor();
				}
				Console.WriteLine("-------------------------------------------");
			}
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write($" ↪️ End of Algo Tests ");
			Console.ResetColor();
			Console.WriteLine("\n");
		}


		private static void MakeTemplateHandEvalTestJson(string pathToTest = @"./Tests/HandEvalUnitTests.json"){
			Console.WriteLine($"- Making Tests JSON file for: \"{pathToTest}\"");
			Deck deck = new();
			HandEvalUnitTest[] testObjects = new HandEvalUnitTest[2];
			List<Card> community = new()
			{
				deck.NextCard(),
				deck.NextCard(),
				deck.NextCard(),
				deck.NextCard(),
				deck.NextCard()
			};
			Pair<Card, Card> playerHand = new(deck.NextCard(), deck.NextCard());
			WinningHand winning = new(HandType.Nothing, community);
			HandEvalUnitTest test = new("My Description", community, playerHand, winning);
			JsonSerializerOptions options = new();
			options.WriteIndented = true;
			testObjects[0] = test;
			testObjects[1] = test;
			string json = JsonSerializer.Serialize(testObjects, options);
			File.WriteAllText(pathToTest, json);
			Console.WriteLine($"- \"{pathToTest}\" has been created!");
		}

		private static void MakeTemplateAlgoTestJson(string pathToTest = @"./Tests/AlgoTests.json"){
			Console.WriteLine($"- Making Tests JSON file for: \"{pathToTest}\"");
			Deck deck = new();
			AlgoUnitTest[] testObjects = new AlgoUnitTest[2];
			List<Card> community = new()
			{
				deck.NextCard(),
				deck.NextCard(),
				deck.NextCard(),
				deck.NextCard(),
				deck.NextCard()
			};
			Pair<Card, Card> playerHand = new(deck.NextCard(), deck.NextCard());
			WinningHand winning = new(HandType.Nothing, community);
			List<Player> players = new(){
				new Player("Test Player 1", new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Clubs, true)),
				new Player("Test Player 2", new Card(14, CardSuit.Diamonds, true), new Card(14, CardSuit.Hearts, true)),
				new Player("Test Player 3", new Card(13, CardSuit.Diamonds, true), new Card(13, CardSuit.Hearts, true)),
			};
			AlgoUnitTest test = new ("Test", players[0].HoleCards,  players[1].HoleCards,  players[2].HoleCards, community, new int[] {0, 1});
			JsonSerializerOptions options = new();
			options.WriteIndented = true;
			testObjects[0] = test;
			testObjects[1] = test;
			string json = JsonSerializer.Serialize(testObjects, options);
			File.WriteAllText(pathToTest, json);
			Console.WriteLine($"- \"{pathToTest}\" has been created!");
		}


		private static bool IsListOfCardsEqual(List<Card> left, List<Card> right)
		{
			Debug.Assert(left.Count == right.Count,"⛔ Testing.AreCardsSame() - left.Count != right.Count");

			for (int i = 0; i < left.Count; i++)
			{
				if (!left.ElementAt(i).Equals(right.ElementAt(i)))
				{
					return false;
				}
			}

			return true;
		}

		private static void SortCardsByValue(List<Card> cards)
		{
			cards.Sort((x, y) => x.Rank.CompareTo(y.Rank));
		}
	}
}