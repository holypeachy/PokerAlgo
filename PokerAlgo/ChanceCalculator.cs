namespace PokerAlgo;
public static class ChanceCalculator
{
    private static readonly double _handStrengthSensitivity = 0.175d; // Logistic Growth Rate of sigmoid
    private static readonly double _baselineWinRate = -1.85d; // Logistic Shift of sigmoid

    private static readonly Dictionary<int, string> _cardPrintLookUp = new()
    {
        {1, "A"}, {2, "2"}, {3, "3"}, {4, "4"}, {5, "5"}, {6, "6"}, {7, "7"}, {8, "8"}, {9, "9"}, {10, "T"},{11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}
    };

    // Returns Win and Tie Values from 0 to 1.0
    public static (double winChance, double tieChance) GetWinningChanceSim(Pair playerHoleCards, List<Card> communityCards, int numOfOpponents, int numberOfSimulatedGames)
    {
        Guards.ArgsWinningChanceSim(playerHoleCards, communityCards, numOfOpponents, numberOfSimulatedGames);

        Deck testDeck = new();
        int timesWon = 0;
        int timesTied = 0;

        Player player = new("Player", new Card(playerHoleCards.First), new Card(playerHoleCards.Second));
        List<Player> allPlayers;
        List<Player> winners;

        List<Card> cardsToRemove = new()
        {
            playerHoleCards.First,
            playerHoleCards.Second
        };
        cardsToRemove.AddRange(communityCards);

        for (int i = 0; i < numberOfSimulatedGames; i++)
        {
            testDeck.ResetDeck();

            testDeck.RemoveCards(cardsToRemove);

            allPlayers = new() { player };

            for (int k = 0; k < numOfOpponents; k++)
            {
                allPlayers.Add(new Player("Simulated Opponent", testDeck.NextCard(), testDeck.NextCard()));
            }

            winners = Algo.GetWinners(allPlayers, communityCards);

            if (winners.Count == 1 && winners[0] == player)
            {
                timesWon++;
            }
            else if (winners.Count > 1 && winners.Contains(player))
            {
                timesTied++;
            }
        }

        return (timesWon / (double)numberOfSimulatedGames, timesTied / (double)numberOfSimulatedGames);
    }

    // Returns Win and Tie Values from 0 to 1.0
    public static (double winChance, double tieChance) GetWinningChancePreFlopSim(Pair playerHoleCards, int numOfOpponents, int numberOfSimulatedGames)
    {
        Guards.ArgsPreFlopSim(playerHoleCards, numOfOpponents, numberOfSimulatedGames);

        Deck testDeck = new();
        int timesWon = 0;
        int timesTied = 0;

        Player player = new("Player", new Card(playerHoleCards.First), new Card(playerHoleCards.Second));
        List<Player> allPlayers;
        List<Player> winners;

        List<Card> cardsToRemove = new()
        {
            playerHoleCards.First,
            playerHoleCards.Second
        };

        for (int i = 0; i < numberOfSimulatedGames; i++)
        {
            testDeck.ResetDeck();

            testDeck.RemoveCards(cardsToRemove);

            List<Card> communityCards = testDeck.NextCards(5);

            allPlayers = new() { player };

            for (int k = 0; k < numOfOpponents; k++)
            {
                allPlayers.Add(new Player("Simulated Opponent", testDeck.NextCard(), testDeck.NextCard()));
            }

            winners = Algo.GetWinners(allPlayers, communityCards);

            if (winners.Count == 1 && winners[0] == player)
            {
                timesWon++;
            }
            else if (winners.Count > 1 && winners.Contains(player))
            {
                timesTied++;
            }
        }

        return (timesWon / (double)numberOfSimulatedGames, timesTied / (double)numberOfSimulatedGames);
    }


    //  Returns Value from 0 to 1.0 from pre-computed data
    public static (double winChance, double tieChance) GetWinningChancePreFlopLookUp(Pair playerHoleCards, int numOfOpponents, IPreFlopDataLoader preFlopDataLoader)
    {
        Guards.ArgsPreFlopLookUp(playerHoleCards, numOfOpponents);

        Dictionary<(string hand, int opponentCount), (double winChance, double tieChance)> PreFlopLookUpTable = preFlopDataLoader.Load();

        string s = $"{_cardPrintLookUp[playerHoleCards.First.Rank]}{_cardPrintLookUp[playerHoleCards.Second.Rank]}" + (playerHoleCards.First.Suit == playerHoleCards.Second.Suit ? "s" : "o");

        (double, double) result;
        try
        {
            result = PreFlopLookUpTable[(s, numOfOpponents)];
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"There is most likely no pre-computed data of that number of opponents. numOfOpponents = {numOfOpponents}");
        }

        return result;
    }

    // Returns Value from 0 to 1.0 | Realistically: 0.1166 to 0.8389
    public static double GetWinningChancePreFlopChen(Pair playerHoleCards)
    {
        Guards.AgainstDuplicateHoleCards(playerHoleCards, nameof(playerHoleCards));

        // ! Sigmoid adjustment
        return 1 / (1 + Math.Exp(-(_handStrengthSensitivity * GetPreFlopChen(playerHoleCards) + _baselineWinRate)));
    }

    // Returns -1 to 20
    public static double GetPreFlopChen(Pair playerHoleCards)
    {
        Guards.AgainstDuplicateHoleCards(playerHoleCards, nameof(playerHoleCards));

        double points = 0;
        Card higherCard;
        Card lowerCard;

        if (playerHoleCards.First.Rank > playerHoleCards.Second.Rank)
        {
            higherCard = playerHoleCards.First;
            lowerCard = playerHoleCards.Second;
        }
        else
        {
            higherCard = playerHoleCards.Second;
            lowerCard = playerHoleCards.First;
        }

        points += higherCard.Rank switch
        {
            14 => 10,
            13 => 8,
            12 => 7,
            11 => 6,
            _ => (double)higherCard.Rank / 2,
        };

        if (higherCard.Rank == lowerCard.Rank)
        {
            points *= 2;
            if (points < 5) points = 5;
        }

        if (higherCard.Suit == lowerCard.Suit)
        {
            points += 2;
        }

        int gap = higherCard.Rank == lowerCard.Rank ? 0 : Math.Abs(higherCard.Rank - lowerCard.Rank - 1);

        points -= (gap >= 4) ? 5 : (gap == 3) ? 4 : gap;

        if ((gap == 0 || gap == 1) && higherCard.Rank != lowerCard.Rank && higherCard.Rank < 12 && lowerCard.Rank < 12)
        {
            points += 1;
        }

        if (points == -1.5d) points = -1;
        else if (points == -0.5d) points = 0;
        else points = Math.Round(points, MidpointRounding.AwayFromZero);

        if (points < -1) throw new InternalPokerAlgoException($"Invariant violated: {nameof(points)} should always be greater than -1 before returning.");

        return points;
    }
    
}