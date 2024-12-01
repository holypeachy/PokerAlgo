using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokerAlgo{
    class Testing{
        public Testing(){
            TestFlushes();
        }

        public void TestFlushes(){
            string json = File.ReadAllText(@"./FlushTests.json");
            TestObject[]? testObjects = JsonSerializer.Deserialize<TestObject[]>(json);
            if(testObjects is null){
                throw new Exception("testObjects array is null. FlushTests.json is empty?");
            }
            foreach (TestObject t in testObjects)
            {
                Console.WriteLine(t.communityCards[0]);
            }
        }

        void FlushJsonMake(){
            Deck deck = new();
            TestObject[] TestObjects = new TestObject[2];
            List<Card> community = new();
            community.Add(deck.NextCard());
            community.Add(deck.NextCard());
            community.Add(deck.NextCard());
            community.Add(deck.NextCard());
            community.Add(deck.NextCard());
            Tuple<Card, Card> hand = new(deck.NextCard(), deck.NextCard());
            WinningHand winning = new(HandType.Nothing, community);
            TestObject object1 = new(community, hand, winning);
            JsonSerializerOptions options = new();
            options.WriteIndented = true;
            TestObjects[0] = object1;
            TestObjects[1] = object1;
            string json = JsonSerializer.Serialize(TestObjects, options);
            File.WriteAllText(@"./FlushTests.json", json);
        }
    }

    class TestObject{
        public List<Card> communityCards { get; set; }
        public Tuple<Card, Card> playerCards { get; set; }

        public WinningHand expectedWinningHand { get; set; }

        public TestObject(List<Card> communityCards, Tuple<Card, Card> playerCards, WinningHand expectedWinningHand) {
            this.communityCards = communityCards;
            this.playerCards = playerCards;
            this.expectedWinningHand = expectedWinningHand;
        }
    }
}