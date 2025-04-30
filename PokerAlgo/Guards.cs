namespace PokerAlgo;

internal static class Guards
{
    public static void ArgsGetWinners(List<Player> players, List<Card> communityCards)
    {
        if (players.Count < 2) throw new ArgumentOutOfRangeException(nameof(players), "There must be at least 2 players.");
        if (communityCards.Count != 5) throw new ArgumentOutOfRangeException(nameof(communityCards), "For the showdown, there must be all 5 community cards.");

        List<Card> allCards = new();
        allCards.AddRange(communityCards);
        foreach (Player p in players)
        {
            allCards.Add(p.HoleCards.First);
            allCards.Add(p.HoleCards.Second);
        }

        bool areUnique = allCards.Count == allCards.Distinct().Count();
        if (areUnique == false) throw new DuplicateCardException($"Either {nameof(players)} or {nameof(communityCards)} arguments have duplicate cards.");

        foreach (Card c in allCards)
        {
            if (c.Rank == 1) throw new LowAcesException("When instantiating Ace cards use rank 14 no 1.");
        }
    }

    public static void ArgsGetWinningHand(List<Card> cards)
    {
        if (cards.Count < 5 || cards.Count > 7) throw new ArgumentOutOfRangeException(nameof(cards), "The list must have 5-7 cards.");

        bool areUnique = cards.Count == cards.Distinct().Count();
        if (areUnique == false) throw new DuplicateCardException($"{nameof(cards)} argument has duplicate cards.");


        foreach (Card c in cards)
        {
            if (c.Rank == 1) throw new LowAcesException("When instantiating Ace cards use rank 14 not 1.");
        }
    }

    public static void ArgsWinningChanceSim(Pair playerHoleCards, List<Card> communityCards, int numOfOpponents, int numberOfSimulatedGames)
    {
        if (communityCards.Count < 3) throw new ArgumentOutOfRangeException(nameof(communityCards), "There should be no less than 3 community cards.");
        if (communityCards.Count > 5) throw new ArgumentOutOfRangeException(nameof(communityCards), "There should be no more than 5 community cards.");
        if (numOfOpponents < 1) throw new ArgumentOutOfRangeException(nameof(numOfOpponents), "There should be at least 1 opponent.");
        if (numberOfSimulatedGames < 100) throw new ArgumentOutOfRangeException(nameof(numberOfSimulatedGames), "Number of simulated games is less than 100. I recommend at least 100 simulated games for a good prediction.");

        List<Card> allCards = new();
        allCards.AddRange(communityCards);
        allCards.Add(playerHoleCards.First);
        allCards.Add(playerHoleCards.Second);

        bool areUnique = allCards.Count == allCards.Distinct().Count();
        if (areUnique == false) throw new DuplicateCardException($"Either {nameof(playerHoleCards)} or {nameof(communityCards)} arguments have duplicate cards.");
    }

    public static void ArgsPreFlopSim(Pair playerHoleCards, int numOfOpponents, int numberOfSimulatedGames)
    {
        if (numOfOpponents < 1) throw new ArgumentOutOfRangeException(nameof(numOfOpponents), "There should be at least 1 opponent.");
        if (numberOfSimulatedGames < 100) throw new ArgumentOutOfRangeException(nameof(numberOfSimulatedGames), "Number of simulated games is less than 100. I recommend at least 100 simulated games for a good prediction.");

        if (playerHoleCards.First.Rank == playerHoleCards.Second.Rank && playerHoleCards.First.Suit == playerHoleCards.Second.Suit) throw new DuplicateCardException($"{nameof(playerHoleCards)} argument has duplicate cards.");
    }

    public static void ArgsPreFlopLookUp(Pair playerHoleCards, int numOfOpponents)
    {
        if (numOfOpponents < 1) throw new ArgumentOutOfRangeException(nameof(numOfOpponents), "There should be at least 1 opponent.");
        if (playerHoleCards.First.Rank == playerHoleCards.Second.Rank && playerHoleCards.First.Suit == playerHoleCards.Second.Suit) throw new DuplicateCardException($"{nameof(playerHoleCards)} argument has duplicate cards.");
    }

    public static void AgainstDuplicateHoleCards(Pair playerHoleCards, string? paramName)
    {
        if (playerHoleCards.First.Rank == playerHoleCards.Second.Rank && playerHoleCards.First.Suit == playerHoleCards.Second.Suit) throw new DuplicateCardException($"{paramName} argument has duplicate cards.");
    }
    
}