namespace PokerAlgo{
    class TestObject
    {
        public string Description { get; set; }
        public Tuple<Card, Card> PlayerCards { get; set; }
        public List<Card> CommunityCards { get; set; }
        public WinningHand ExpectedWinningHand { get; set; }

        public TestObject(string description, List<Card> communityCards, Tuple<Card, Card> playerCards, WinningHand expectedWinningHand)
        {
            this.Description = description;
            this.CommunityCards = communityCards;
            this.PlayerCards = playerCards;
            this.ExpectedWinningHand = expectedWinningHand;
        }
    }
}