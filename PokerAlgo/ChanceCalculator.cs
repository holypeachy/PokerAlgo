using System.Diagnostics;

namespace PokerAlgo
{
    public static class ChanceCalculator
    {
        // Returns Value from 0 to 1.0
        public static double GetWinningChance(Pair<Card, Card> playerHoleCards, List<Card> communityCards, int numOfPlayers, int numberOfSimulatedGames)
        {
            Debug.Assert(communityCards.Count >= 3, "⛔ communityCards.Count is less than 3");

            Deck testDeck = new();
            int numberOfGames = numberOfSimulatedGames;
            int timesWon = 0;

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
            }

            return timesWon / (double)numberOfGames;
        }

        // Returns Value from 0 to 1.0
        public static double GetWinningChancePreFlop(Pair<Card, Card> playerHoleCards)
        {
            // ! Bill Chen Formula
            double totalPoints = 0;
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

            switch (higherCard.Rank)
            {
                case 14:
                    totalPoints += 10;
                    break;
                case 13:
                    totalPoints += 8;
                    break;
                case 12:
                    totalPoints += 7;
                    break;
                case 11:
                    totalPoints += 6;
                    break;
                default:
                    totalPoints+= (double)higherCard.Rank / 2 ;
                    break;
            }

            if(higherCard.Rank == lowerCard.Rank)
            {
                totalPoints *= 2;
                if(totalPoints < 5) totalPoints = 5;
            }
            if(higherCard.Suit == lowerCard.Suit) totalPoints += 2;
            int gap = Math.Abs(higherCard.Rank - lowerCard.Rank);
            
            if(gap >= 4) totalPoints -= 5;
            else if(gap == 3) totalPoints -= 4;
            else if(gap == 1 || gap == 2) totalPoints -= gap;

            if((gap == 0 || gap == 1) && higherCard.Rank < 12 && lowerCard.Rank < 12) totalPoints += 1;

            totalPoints = Math.Round(totalPoints, MidpointRounding.AwayFromZero);
            totalPoints += 5.4d;

            return totalPoints / 25.4d * 0.85d;
        }
    }
}