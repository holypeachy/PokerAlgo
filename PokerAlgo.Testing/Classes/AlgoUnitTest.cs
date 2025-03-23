namespace PokerAlgo.Testing;

class AlgoUnitTest
{
    public string Description { get; set; }
    public Pair<Card, Card> Player1 {get; set;}
    public Pair<Card, Card> Player2 {get; set;}
    public Pair<Card, Card> Player3 {get; set;}
    public List<Card> CommunityCards { get; set; }
    public int[] IndicesOfWinners { get; set; }

    public AlgoUnitTest(string description, Pair<Card, Card> player1, Pair<Card, Card> player2, Pair<Card, Card> player3, List<Card> communityCards, int[] indicesOfWinners)
    {
        Description = description;
        Player1 = player1;
        Player2 = player2;
        Player3 = player3;
        CommunityCards = communityCards;
        IndicesOfWinners = indicesOfWinners;
    }

    public override string ToString()
    {
        string p = $"\t\t{Player1}\n\t\t{Player2}\n\t\t{Player3}";
        string c = string.Join(' ', CommunityCards);
        string w = string.Join(", ", IndicesOfWinners);

        return $"AlgoUnitTest: {Description}\n\tPlayers:\n{p}\n\tCommunity Cards:\n\t\t{c}\n\tExpected Winner(s):\n\t{w}";
    }
}