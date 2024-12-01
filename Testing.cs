using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokerAlgo{
    delegate void AlgoFunction(List<Card> cards, Player player);
    
    class Testing{
        private string pathToFlush = @"./Tests/FlushTests.json";
        private string pathToStraight = @"./Tests/StraightTests.json";


        public Testing(){
            PerformFinderTest("FlushFinder", pathToFlush, Algo.FlushFinder);
            PerformFinderTest("StraightFinder", pathToStraight, Algo.StraightFinder);
        }

        public void TestFlushes(){
            string json = File.ReadAllText(@"./FlushTests.json");
            TestObject[]? testObjects = JsonSerializer.Deserialize<TestObject[]>(json);
            if(testObjects is null){
                throw new Exception("testObjects array is null. FlushTests.json is empty?");
            }

            int testCount = 1;
            foreach (TestObject test in testObjects)
            {
                Player player = new ("Test", test.PlayerCards.Item1, test.PlayerCards.Item2);

                List<Card> combinedCards = new();
                combinedCards.Add(player.Hand.Item1);
                combinedCards.Add(player.Hand.Item2);
                foreach (Card c in test.CommunityCards)
                {
                    combinedCards.Add(c);
                }
                combinedCards = combinedCards.OrderBy(x => x.Value).ToList();
                Algo.FlushFinder(combinedCards, player);

                WinningHand expected = test.ExpectedWinningHand;
                WinningHand actual = player.WinningHands[0];

                bool passed = true;
                if(expected.Type == actual.Type && expected.Cards.Count == actual.Cards.Count){
                    for (int i = 0; i < expected.Cards.Count; i++)
                    {
                        if(!expected.Cards[i].Equals(actual.Cards[i])){
                            passed = false;
                        }
                    }
                }
                else{
                    passed = false;
                }
                // Console.WriteLine(test.Description);
                Console.WriteLine($"TEST {testCount++}:" + (passed ? " PASSED ✅" : $" FAILED ❌: {test.Description}"));
            }
        }

        public void PerformFinderTest(string testName, string pathToTest, AlgoFunction function){
            Console.WriteLine($"---{testName}---");
            string json = File.ReadAllText(pathToTest);
            TestObject[]? testObjects = JsonSerializer.Deserialize<TestObject[]>(json);
            if (testObjects is null)
            {
                throw new Exception($"testObjects array is null. is {pathToTest} is empty?");
            }

            int testCount = 1;
            foreach (TestObject test in testObjects)
            {
                Player player = new("Test", test.PlayerCards.Item1, test.PlayerCards.Item2);

                List<Card> combinedCards = new();
                combinedCards.Add(player.Hand.Item1);
                combinedCards.Add(player.Hand.Item2);
                foreach (Card c in test.CommunityCards)
                {
                    combinedCards.Add(c);
                }
                combinedCards = combinedCards.OrderBy(x => x.Value).ToList();
                function(combinedCards, player);

                WinningHand expected = test.ExpectedWinningHand;
                WinningHand actual = player.WinningHands[0];

                bool passed = true;
                if (expected.Type == actual.Type && expected.Cards.Count == actual.Cards.Count)
                {
                    for (int i = 0; i < expected.Cards.Count; i++)
                    {
                        if (!expected.Cards[i].Equals(actual.Cards[i]))
                        {
                            passed = false;
                        }
                    }
                }
                else
                {
                    passed = false;
                }
                // Console.WriteLine(test.Description);
                Console.WriteLine($"TEST {testCount++}:" + (passed ? " PASSED ✅" : $" FAILED ❌: {test.Description}"));
            }
        }

        public void MakeJson(string pathToTest){
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
            TestObject object1 = new("My Description",community, hand, winning);
            JsonSerializerOptions options = new();
            options.WriteIndented = true;
            TestObjects[0] = object1;
            TestObjects[1] = object1;
            string json = JsonSerializer.Serialize(TestObjects, options);
            File.WriteAllText(pathToTest, json);
            Console.WriteLine($"- \"{pathToTest}\" has been created!");
        }
    }
}