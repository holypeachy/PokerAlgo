namespace PokerAlgo
{
    internal static class ChanceCalculator
    {
        public static double CalculateChances(Player targetPlayer, List<Card> communityCards)
        {
            Deck testDeck = new();
            int numberOfGames = 10000;
            int timesWon = 0;

            HandEvaluator handEvaluator = new();

            Player randomPlayer;
            for (int i = 0; i < numberOfGames; i++)
            {
                testDeck.ResetDeck();
                Card First = new (targetPlayer.HoleCards.First);
                Card Second = new (targetPlayer.HoleCards.Second);
                First.IsPlayerCard = false;
                Second.IsPlayerCard = false;
                testDeck.Cards.Remove(First);
                testDeck.Cards.Remove(Second);
                testDeck.Cards = testDeck.Cards.Except(communityCards).ToList();

                randomPlayer = new("Random", testDeck.NextCard(), testDeck.NextCard());

                List<Card> cards = new()
                {
                    randomPlayer.HoleCards.First,
                    randomPlayer.HoleCards.Second
                };
                cards.AddRange(communityCards);

                randomPlayer.WinningHand = handEvaluator.GetWinningHand(cards);


                int result = Algo.ComparePlayerHands(targetPlayer, randomPlayer);
                if (result == -1){
                    timesWon++;
                }
            }

            return timesWon / (double)numberOfGames;
        }
    }
}