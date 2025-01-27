using System.Text.Json;
using PokerAlgo;

namespace Project
{
	class Testing {
		private delegate void AlgoFunction(List<Card> cards, Player player);

		// private string pathToFlush = @"C:\Users\Frank\Code\poker-algo\Tests\FlushTests.json";
		// private string pathToStraight = @"C:\Users\Frank\Code\poker-algo\Tests\Tests\StraightTests.json";
		// private string pathToMultiple = @"C:\Users\Frank\Code\poker-algo\Tests\Tests\MultipleTests.json";

		private static bool _debugEnable = false;


		public Testing() {
			// ! PerformHandEvalTests("EvaluateFlush", pathToFlush, Algo.EvaluateFlush);
			// ! PerformHandEvalTests("EvaluateStraight", pathToStraight, Algo.EvaluateStraight);
			// ! PerformHandEvalTests("EvaluateMultiples", pathToMultiple, Algo.EvaluateMultiples);
		}

		private void PerformHandEvalTests(string testName, string pathToTest, AlgoFunction function)
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine($"🧪 --{testName}---");
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

				function(cards, player);

				WinningHand? expectedHand = test.ExpectedHand;
				WinningHand? actualHand = player.WinningHand;

				if (expectedHand is null) expectedHand = new(HandType.Nothing, new List<Card>());
				if (actualHand is null) actualHand = new(HandType.Nothing, new List<Card>());

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
					if (expectedHand is null) expectedHand = new( HandType.Nothing, new List<Card>());
					if (actualHand is null) actualHand = new( HandType.Nothing, new List<Card>());

					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"TEST FAILED ❌");
					Console.WriteLine($"\tExpected: {expectedHand.Type}" + (expectedHand.Type == HandType.Nothing ? "" : $"Cards: {string.Join(' ', expectedHand.Cards)}") );
					Console.WriteLine($"\tActual: {actualHand.Type}" + (actualHand.Type == HandType.Nothing ? "" : $"Cards: {string.Join(' ', actualHand.Cards)}") );
					Console.ResetColor();
				}
			}
		}

		private bool IsListOfCardsEqual(List<Card> left, List<Card> right)
		{
			if (left.Count != right.Count) throw new Exception("⛔ Testing.AreCardsSame() - left.Count != right.Count");

			for (int i = 0; i < left.Count; i++)
			{
				if (!left.ElementAt(i).Equals(right.ElementAt(i)))
				{
					return false;
				}
			}

			return true;
		}

		private void MakeTemplateTestJson(string pathToTest){
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

		private static void SortCardsByValue(List<Card> cards)
		{
			cards.Sort((x, y) => x.Rank.CompareTo(y.Rank));
		}
	}
}