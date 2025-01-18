using System.Text.Json;

namespace PokerAlgo{
	delegate void AlgoFunction(List<Card> cards, Player player);
	
	class Testing{
		private string pathToFlush = @"./Tests/FlushTests.json";
		private string pathToStraight = @"./Tests/StraightTests.json";
		private string pathToMultiple = @"./Tests/MultipleTests.json";

		private static bool _debugEnable = false;


		public Testing(){
			PerformFinderTest("FlushFinder", pathToFlush, Algo.EvaluateFlush);
			PerformFinderTest("StraightFinder", pathToStraight, Algo.EvaluateStraight);
			PerformFinderTest("MultipleFinder", pathToMultiple, Algo.EvaluateMultiples);
		}

		public void PerformFinderTest(string testName, string pathToTest, AlgoFunction function)
		{
			if (!Algo._unitTestingEnable)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("⚠️ Variable \"unitTestingEnable\" is FALSE and will be set to TRUE!");
				Algo._unitTestingEnable = true;
				Console.ResetColor();
			}

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine($"🧪 --{testName}---");
			Console.ResetColor();

			string json = File.ReadAllText(pathToTest);
			TestObject[]? testObjects = JsonSerializer.Deserialize<TestObject[]>(json);
			if (testObjects is null)
			{
				throw new Exception($"testObjects array is null. is {pathToTest} is empty?");
			}

			int testCount = 1;
			bool passed = true;
			foreach (TestObject test in testObjects)
			{
				Console.WriteLine($"Running Test {testCount++}: {test.Description}");
				if (_debugEnable)
				{
					Console.WriteLine("Current Object:");
					Console.WriteLine(test);
					Console.WriteLine();
				}
				WinningHand expectedHand = new(HandType.Nothing, new List<Card>());
				WinningHand actualHand = new(HandType.Nothing, new List<Card>());
				Player player = new("Test", test.PlayerCards.Item1, test.PlayerCards.Item2);

				List<Card> combinedCards = new()
				{
					player.Hand.First,
					player.Hand.Second
				};
				combinedCards.AddRange(test.CommunityCards);

				combinedCards = combinedCards.OrderBy(x => x.Value).ToList();
				AddLowAces(combinedCards);
				function(combinedCards, player);

				List<WinningHand> expectedHands = test.ExpectedWinningHands;
				WinningHand actualHands = player.WinningHand;

				passed = true;
				if (expectedHands.Count == 1)
				{
					for (int handIndex = 0; handIndex < expectedHands.Count; handIndex++)
					{
						expectedHand = expectedHands[handIndex];
						actualHand = actualHands;

						if (expectedHand.Type == actualHand.Type && expectedHand.Cards.Count == actualHand.Cards.Count)
						{
							for (int cardIndex = 0; cardIndex < expectedHand.Cards.Count; cardIndex++)
							{
								if (!IsSuitRelevant(expectedHand.Type))
								{
									if (!expectedHand.Cards[cardIndex].EqualsInValue(actualHand.Cards[cardIndex]))
									{
										passed = false;
										break;
									}
								}
								else
								{
									if (!expectedHand.Cards[cardIndex].Equals(actualHand.Cards[cardIndex]))
									{
										passed = false;
										break;
									}
								}
							}
						}
						else
						{
							passed = false;
							break;
						}

						if (!passed)
						{
							break;
						}
					}
				}
				else
				{
					passed = false;
				}

				// Console.WriteLine(test.Description);
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
					Console.WriteLine($"   Expected: {expectedHand.Type}, Cards: {string.Join(", ", expectedHand.Cards)}");
					Console.WriteLine($"   Actual: {actualHand.Type}, Cards: {string.Join(", ", actualHand.Cards)}");
					Console.ResetColor();
				}
			}
			Console.WriteLine();
		}


		private void AddLowAces(List<Card> cards)
		{
			List<Card> acesToAdd = new();
			foreach (Card c in cards)
			{
				if (c.Value == 14)
				{
					acesToAdd.Add(new Card(1, c.Suit, c.IsPlayerCard));
				}
			}
			cards.InsertRange(0, acesToAdd);
		}

		public void MakeTemplateTestJson(string pathToTest){
			Console.WriteLine($"- Making Tests JSON file for: \"{pathToTest}\"");
			Deck deck = new();
			TestObject[] TestObjects = new TestObject[2];
			List<Card> community = new()
			{
				deck.NextCard(),
				deck.NextCard(),
				deck.NextCard(),
				deck.NextCard(),
				deck.NextCard()
			};
			Tuple<Card, Card> hand = new(deck.NextCard(), deck.NextCard());
			WinningHand winning = new(HandType.Nothing, community);
			List<WinningHand> hands = new List<WinningHand>{winning};
			TestObject object1 = new("My Description",community, hand, hands);
			JsonSerializerOptions options = new();
			options.WriteIndented = true;
			TestObjects[0] = object1;
			TestObjects[1] = object1;
			string json = JsonSerializer.Serialize(TestObjects, options);
			File.WriteAllText(pathToTest, json);
			Console.WriteLine($"- \"{pathToTest}\" has been created!");
		}


		private bool IsSuitRelevant(HandType type)
		{
			return type == HandType.RoyalFlush || type == HandType.StraightFlush || type == HandType.Flush;
		}


		// ! For converting the json tests from one type of test object to another
		public void ConvertJson(string pathToTest){
			// string json = File.ReadAllText(pathToTest);
			// TestObject[]? testObjects = JsonSerializer.Deserialize<TestObject[]>(json);
			// TestObject2[] testObjects2 = new TestObject2[testObjects.Length];
			// for (int i = 0; i < testObjects.Length; i++)
			// {
			//     TestObject test = testObjects[i];
			//     List<WinningHand> winningHands = new()
			//     {
			//         test.ExpectedWinningHand
			//     };
			//     testObjects2[i] = new TestObject2(test.Description, test.CommunityCards, test.PlayerCards, winningHands);
			// }

			// JsonSerializerOptions options = new();
			// options.WriteIndented = true;
			// json = JsonSerializer.Serialize(testObjects2, options);
			// File.WriteAllText(pathToTest, json);
		}

		// ! Depricated
		// public void TestFlushes()
		// {
		// 	string json = File.ReadAllText(@"./FlushTests.json");
		// 	TestObject[]? testObjects = JsonSerializer.Deserialize<TestObject[]>(json);
		// 	if (testObjects is null)
		// 	{
		// 		throw new Exception("testObjects array is null. FlushTests.json is empty?");
		// 	}

		// 	int testCount = 1;
		// 	foreach (TestObject test in testObjects)
		// 	{
		// 		Player player = new("Test", test.PlayerCards.Item1, test.PlayerCards.Item2);

		// 		List<Card> combinedCards = new();
		// 		combinedCards.Add(player.Hand.First);
		// 		combinedCards.Add(player.Hand.First);
		// 		foreach (Card c in test.CommunityCards)
		// 		{
		// 			combinedCards.Add(c);
		// 		}
		// 		combinedCards = combinedCards.OrderBy(x => x.Value).ToList();
		// 		Algo.FindFlush(combinedCards, player);

		// 		WinningHand expected = test.ExpectedWinningHands[0];
		// 		WinningHand actual = player.WinningHand;

		// 		bool passed = true;
		// 		if (expected.Type == actual.Type && expected.Cards.Count == actual.Cards.Count)
		// 		{
		// 			for (int i = 0; i < expected.Cards.Count; i++)
		// 			{
		// 				if (!expected.Cards[i].Equals(actual.Cards[i]))
		// 				{
		// 					passed = false;
		// 				}
		// 			}
		// 		}
		// 		else
		// 		{
		// 			passed = false;
		// 		}
		// 		// Console.WriteLine(test.Description);
		// 		Console.WriteLine($"TEST {testCount++}:" + (passed ? " PASSED ✅" : $" FAILED ❌: {test.Description}"));
		// 	}
		// }

	}
}