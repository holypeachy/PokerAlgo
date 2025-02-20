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
        public static double GetWinningChance(Pair<Card, Card> playerHoleCards, int numOfPlayers, int numberOfSimulatedGames)
        {
            // TODO: Add a lookup table for Pre-Flop chances of winning.
            throw new NotImplementedException();
        }
    }
}