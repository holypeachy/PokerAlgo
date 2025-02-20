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
		}

		private static void TestHandEvaluator(string pathToTest = @"./Tests/HandEvalUnitTests.json")
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine($"🧪 --Hand Evaluator Tests---");
			Console.ResetColor();

			string json = File.ReadAllText(pathToTest);
			HandEvalUnitTest[]? testObjects = JsonSerializer.Deserialize<HandEvalUnitTest[]>(json);

			if (testObjects is null)
			{
				throw new Exception($"testObjects array is null. is {pathToTest} is empty?");
			}

			int testCount = 1;
			bool passed = true;
			foreach (HandEvalUnitTest test in testObjects)
			{
				Console.WriteLine($"Running Test {testCount++}: {test.Description}");
				if (_debugEnable)
				{
					Console.WriteLine(test);
					Console.WriteLine();
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
			}
		}


		private static void MakeTemplateHandEvalJson(string pathToTest){
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
			options.WriteIndented = false;
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