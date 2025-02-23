using System.Diagnostics;

namespace PokerAlgo
{
    public static class ChanceCalculator
    {
        private static readonly double _handStrengthSensitivity = 0.175d; // Logistic Growth Rate of sigmoid
        private static readonly double _baselineWinRate = -1.85d; // Logistic Shift of sigmoid

        // Returns Win and Tie Values from 0 to 1.0
        public static Tuple<double, double> GetWinningChance(Pair<Card, Card> playerHoleCards, List<Card> communityCards, int numOfPlayers, int numberOfSimulatedGames)
        {
            Debug.Assert(communityCards.Count >= 3, "⛔ communityCards.Count is less than 3");

            Deck testDeck = new();
            int numberOfGames = numberOfSimulatedGames;
            int timesWon = 0;
            int timesTied = 0;

            Player player = new("Player", new Card(playerHoleCards.First), new Card(playerHoleCards.Second));
            List<Player> allPlayers;
            List<Player> winners;

            for (int i = 0; i < numberOfGames; i++)
            {
                testDeck.ResetDeck();

                List<Card> cardsToRemove = new List<Card>()
                {
                    playerHoleCards.First,
                    playerHoleCards.Second
                };
                cardsToRemove.AddRange(communityCards);

                testDeck.RemoveCards(cardsToRemove);

                allPlayers = new() { player };

                for (int k = 0; k < numOfPlayers; k++)
                {
                    allPlayers.Add( new Player("Simulated Opponent", testDeck.NextCard(), testDeck.NextCard()) );
                }

                winners = Algo.GetWinners(allPlayers, communityCards);

                if (winners.Count == 1 && winners.ElementAt(0) == player)
                {
                    timesWon++;
                }
                else if(winners.Count > 1 && winners.Contains(player))
                {
                    timesTied++;
                }
            }

            return new Tuple<double, double>(timesWon / (double)numberOfGames, timesTied / (double)numberOfGames);
        }

        // Returns Value from 0 to 1.0 | Realistically: 0.1166 to 0.8389
        public static double GetWinningChancePreFlop(Pair<Card, Card> playerHoleCards)
        {
            // ! Sigmoid adjustment
            return 1 / (1 + Math.Exp(-(_handStrengthSensitivity * GetPreFlopChen(playerHoleCards) + _baselineWinRate)));
        }
    
        // Returns -1 to 20
        public static double GetPreFlopChen(Pair<Card, Card> playerHoleCards)
        {
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
                if(points < 5) points = 5;
            }

            if(higherCard.Suit == lowerCard.Suit)
            {
                points += 2;
            }

            int gap = higherCard.Rank == lowerCard.Rank ? 0 : Math.Abs(higherCard.Rank - lowerCard.Rank - 1);
            
            if (gap >= 4) points -= 5;
            else if(gap == 3) points -= 4;
            else if(gap == 1 || gap == 2)
            {
                points -= gap;
            }

            if ( (gap == 0 || gap == 1) && higherCard.Rank != lowerCard.Rank && higherCard.Rank < 12 && lowerCard.Rank < 12)
            {
                points += 1;
            }
            
            if(points == -1.5d) points = -1;
            else if(points == -0.5d) points = 0;
            else points = Math.Round(points, MidpointRounding.AwayFromZero);

            Debug.Assert(points >= -1, "totalPoints should always be greater than -1");
            return points;
        }
    }
}