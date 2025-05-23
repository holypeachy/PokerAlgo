namespace Sandbox;

class HandEvalTest
{
    public string Description { get; set; }
    public Pair PlayerCards { get; set; }
    public List<Card> CommunityCards { get; set; }
    public WinningHand ExpectedHand { get; set; }

    public HandEvalTest(string description, List<Card> communityCards, Pair playerCards, WinningHand expectedHand)
    {
        this.Description = description;
        this.CommunityCards = communityCards;
        this.PlayerCards = playerCards;
        this.ExpectedHand = expectedHand;
    }

    public override string ToString()
    {
        string temp = $"Expected Hand: {ExpectedHand.Type} | { string.Join(' ', ExpectedHand.Cards)}";
        return $"HandEvalUnitTest: {Description}\n\tPlayer Cards: {PlayerCards}\n\tCommunity Cards: {string.Join(" ", CommunityCards)}\n\t{temp}";
    }
}