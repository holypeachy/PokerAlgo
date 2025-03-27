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
    public static Tuple<double, double> GetWinningChanceSim(Pair<Card, Card> playerHoleCards, List<Card> communityCards, int numOfOpponents, int numberOfSimulatedGames)
    {
        if (communityCards.Count < 3) throw new ArgumentException("⛔ communityCards.Count is less than 3");
        if (communityCards.Count > 5) throw new ArgumentException("⛔ communityCards.Count is more than 5");
        if (numOfOpponents < 1) throw new ArgumentException("⛔ numOfOpponents is less than 2");
        if (numberOfSimulatedGames < 100) throw new ArgumentException("⛔ numberOfSimulatedGames is less than 100. I recommend at least 100 simulated games for a better prediction.");


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

            if (winners.Count == 1 && winners.ElementAt(0) == player)
            {
                timesWon++;
            }
            else if (winners.Count > 1 && winners.Contains(player))
            {
                timesTied++;
            }
        }

        return new Tuple<double, double>(timesWon / (double)numberOfSimulatedGames, timesTied / (double)numberOfSimulatedGames);
    }

    // Returns Win and Tie Values from 0 to 1.0
    public static Tuple<double, double> GetWinningChancePreFlopSim(Pair<Card, Card> playerHoleCards, int numOfOpponents, int numberOfSimulatedGames)
    {
        if (numOfOpponents < 1) throw new ArgumentException("⛔ numOfOpponents is less than 2");
        if (numberOfSimulatedGames < 100) throw new ArgumentException("⛔ numberOfSimulatedGames is less than 100. I recommend at least 100 simulated games for a better prediction.");

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

            List<Card> communityCards = new()
            {
                testDeck.NextCard(),
                testDeck.NextCard(),
                testDeck.NextCard(),
                testDeck.NextCard(),
                testDeck.NextCard()
            };

            testDeck.RemoveCards(cardsToRemove);

            allPlayers = new() { player };

            for (int k = 0; k < numOfOpponents; k++)
            {
                allPlayers.Add(new Player("Simulated Opponent", testDeck.NextCard(), testDeck.NextCard()));
            }

            winners = Algo.GetWinners(allPlayers, communityCards);

            if (winners.Count == 1 && winners.ElementAt(0) == player)
            {
                timesWon++;
            }
            else if (winners.Count > 1 && winners.Contains(player))
            {
                timesTied++;
            }
        }

        return new Tuple<double, double>(timesWon / (double)numberOfSimulatedGames, timesTied / (double)numberOfSimulatedGames);
    }

    // Returns Value from 0 to 1.0 | Realistically: 0.1166 to 0.8389
    public static double GetWinningChancePreFlopChen(Pair<Card, Card> playerHoleCards)
    {
        // ! Sigmoid adjustment
        return 1 / (1 + Math.Exp(-(_handStrengthSensitivity * GetPreFlopChen(playerHoleCards) + _baselineWinRate)));
    }

    // Returns -1 to 20
    public static double GetPreFlopChen(Pair<Card, Card> playerHoleCards)
    {
        if (playerHoleCards.First.Rank == playerHoleCards.Second.Rank && playerHoleCards.First.Suit == playerHoleCards.Second.Suit) throw new ArgumentException("⛔ Player hole cards are the same, this will never happen in a real game.");

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

        if (points < -1) throw new Exception("ChanceCalculator.GetPreFlopChen() - totalPoints should always be greater than -1 before returning. Chen formula is not implemented correctly, contact package developer.");
        // Debug.Assert(points >= -1, "totalPoints should always be greater than -1");
        return points;
    }


    // TODO
    // ! Returns Pre-Computed Chances of winning
    public static Tuple<double, double> GetWinningChancePreFlopLookUp(Pair<Card, Card> playerCards, int numOfOpponents, string dataPath)
    {
        Dictionary<string, Tuple<double, double>> PreFlopDictionary = LoadDictionary(dataPath);

        string s = $"{_cardPrintLookUp[playerCards.First.Rank]}{_cardPrintLookUp[playerCards.Second.Rank]}" + (playerCards.First.Suit == playerCards.Second.Suit ? "s" : "o");
        return PreFlopDictionary[s];
    }

    private static Dictionary<string, Tuple<double, double>>? LoadDictionary(string filePath)
    {
        string[] data;
        Dictionary<string, Tuple<double, double>> dict = new();

        data = File.ReadAllLines(filePath);

        string[] line;
        for (int index = 0; index < data.Length; index++)
        {
            line = data[index].Split(' ');
            dict[line[0]] = new Tuple<double, double>(double.Parse(line[1]), double.Parse(line[2]));
        }

        return dict;
    }
}