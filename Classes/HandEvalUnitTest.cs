namespace PokerAlgo{
    class HandEvalUnitTest
    {
        public string Description { get; set; }
        public Pair<Card, Card> PlayerCards { get; set; }
        public List<Card> CommunityCards { get; set; }
        public WinningHand? ExpectedHand { get; set; }

        public HandEvalUnitTest(string description, List<Card> communityCards, Pair<Card, Card> playerCards, WinningHand expectedHand)
        {
            this.Description = description;
            this.CommunityCards = communityCards;
            this.PlayerCards = playerCards;
            this.ExpectedHand = expectedHand;
        }

        public override string ToString()
        {
            string temp = ExpectedHand is null ? "Expected Hand: Nothing" : $"Expected Hand: {ExpectedHand.Type} | { string.Join(' ', ExpectedHand.Cards)}";
            return $"HandEvalUnitTest: {Description}\n\tPlayer Cards: {PlayerCards}\n\tCommunity Cards: {string.Join(" ", CommunityCards)}\n\t{temp}";
        }
    }
}