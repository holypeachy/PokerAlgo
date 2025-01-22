namespace PokerAlgo
{
    internal static class ChanceCalculator
    {
        public static double CalculateChances(Pair<Card, Card> playerHoleCards, List<Card> communityCards, int numOfPlayers)
        {
            Deck testDeck = new();
            int numberOfGames = 2000;
            int timesWon = 0;
            int timesTied = 0;

            HandEvaluator handEvaluator = new();

            Player player = new("Player", new Card(playerHoleCards.First), new Card(playerHoleCards.Second));
            for (int i = 0; i < numberOfGames; i++)
            {
                List<Player> players = new()
                {
                    player
                };
                testDeck.ResetDeck();
                testDeck.Cards.RemoveAll(x => x.Equals(playerHoleCards.First));
                testDeck.Cards.RemoveAll(x => x.Equals(playerHoleCards.Second));

                testDeck.Cards.RemoveAll(x => x.Equals(communityCards[0]));
                testDeck.Cards.RemoveAll(x => x.Equals(communityCards[1]));
                testDeck.Cards.RemoveAll(x => x.Equals(communityCards[2]));

                if(communityCards.Count == 4)
                {
                    testDeck.Cards.RemoveAll(x => x.Equals(communityCards[3]));
                }
                else if(communityCards.Count == 5)
                {
                    testDeck.Cards.RemoveAll(x => x.Equals(communityCards[3]));
                    testDeck.Cards.RemoveAll(x => x.Equals(communityCards[4]));
                }


                for (int k = 0; k < numOfPlayers; k++)
                {
                    players.Add(new Player("Opponent", testDeck.NextCard(), testDeck.NextCard()));
                }

                List<Player> winners = Algo.GetWinners(players, communityCards);

                if (winners.Count == 1 && winners.Contains(player))
                {
                    timesWon++;
                }
                else if(winners.Count > 1 && winners.Contains(player))
                {
                    timesTied++;
                }
            }

            Console.WriteLine($"ChanceCalculator.CalculateChances() - Number of Simulations: {numberOfGames}");
            // Console.WriteLine("\tChances of Tie: " + ((float)timesTied) / numberOfGames * 100 + "%");
            return timesWon / (double)numberOfGames;
        }
    }
}