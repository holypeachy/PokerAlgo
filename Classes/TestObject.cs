namespace PokerAlgo{
    class TestObject
    {
        public string Description { get; set; }
        public Tuple<Card, Card> PlayerCards { get; set; }
        public List<Card> CommunityCards { get; set; }
        public List<WinningHand> ExpectedWinningHands { get; set; }

        public TestObject(string description, List<Card> communityCards, Tuple<Card, Card> playerCards, List<WinningHand> expectedWinningHands)
        {
            this.Description = description;
            this.CommunityCards = communityCards;
            this.PlayerCards = playerCards;
            this.ExpectedWinningHands = expectedWinningHands;
        }
    }
}