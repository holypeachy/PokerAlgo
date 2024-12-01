namespace PokerAlgo
{
    static class Algo
    {
        private static bool debugEnable = false;
        private static bool testingEnable = true;

        public static void FindWinner(List<Player> players, List<Card> community)
        {
            // Lot of fancy code stuff
            // DeterminePlayerHands(players[0], community);
            Player testPlayer = new Player("Test",
            new Card(2, CardSuit.Hearts, true),
            new Card(11, CardSuit.Hearts, true));

            List<Card> testCom = new List<Card>(){
            new Card(2, CardSuit.Diamonds, false),
            new Card(5, CardSuit.Hearts, false),
            new Card(7, CardSuit.Hearts, false),
            new Card(8, CardSuit.Spades, false),
            new Card(10, CardSuit.Hearts, false)};

            DeterminePlayerHands(testPlayer, testCom);
        }

        private static void DeterminePlayerHands(Player player, List<Card> community)
        {
            List<Card> cards = new();

            cards.Add(player.Hand.Item1);
            cards.Add(player.Hand.Item2);
            foreach (Card c in community)
            {
                cards.Add(c);
            }

            cards = cards.OrderBy(x => x.Value).ToList();

            // Console.ReadLine();
            // Console.Clear();
            Console.WriteLine("All Cards: ");
            foreach (Card c in cards)
            {
                Console.WriteLine($"{c}" + (c.IsPlayerCard ? "player" : "") );
            }

            FlushFinder(cards, player);
        }

        public static void FlushFinder(List<Card> cards, Player player){
            List<Card> currentWinnerHand = new();
            List<Card> flushCards = new();

            // ! Flush?
            bool isFlush = false;
            CardSuit flushSuit = CardSuit.Spades;

            // Do we have a flush?
            foreach (CardSuit currentSuit in Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>())
            {
                int count = 0;
                foreach (Card c in cards)
                {
                    if (c.Suit == currentSuit)
                    {
                        count++;
                    }
                }

                if (count >= 5)
                {
                    isFlush = true;
                    flushSuit = currentSuit;
                    break;
                }
            }

            if(!isFlush){
                // ! For Testing
                if(debugEnable){
                    Console.WriteLine("-NO FLUSH FOUND-");
                }
                if(testingEnable){
                    WinningHand t = new(HandType.Nothing, new List<Card>());
                    player.WinningHands.Add(t);
                }
                return;
            }

            // * Extract all flush cards
            if(debugEnable) Console.WriteLine("\nFlush Cards:");
            foreach (Card c in cards)
            {
                if(c.Suit == flushSuit){
                    flushCards.Add(new Card(c.Value, c.Suit, c.IsPlayerCard));
                    if(debugEnable) Console.WriteLine(c);

                }
            }

            // Do we have a royal flush?
            bool isRoyalFlush = true;
            bool[] royalMatches = { false, false, false, false, false };
            // ! Royal Flush?
            foreach (Card c in flushCards)
            {
                switch (c.Value)
                {
                    case 1:
                        royalMatches[0] = true;
                        currentWinnerHand.Add(c);
                        break;
                    case 10:
                        royalMatches[1] = true;
                        currentWinnerHand.Add(c);
                        break;
                    case 11:
                        royalMatches[2] = true;
                        currentWinnerHand.Add(c);
                        break;
                    case 12:
                        royalMatches[3] = true;
                        currentWinnerHand.Add(c);
                        break;
                    case 13:
                        royalMatches[4] = true;
                        currentWinnerHand.Add(c);
                        break;
                    default:
                        break;
                }
            }

            foreach (bool condition in royalMatches)
            {
                if (condition == false)
                {
                    isRoyalFlush = false;
                    break;
                }
            }

            if (isRoyalFlush && ContainsPlayerCard(currentWinnerHand))
            {
                WinningHand tempWinning = new(HandType.RoyalFlush, currentWinnerHand);
                player.WinningHands.Add(tempWinning);
                if(debugEnable){
                    Console.Write("\nROYAL FLUSH: ");
                    foreach (Card c in currentWinnerHand)
                    {
                        Console.Write($"{c} ");
                    }
                }
                return; // Return if Royal Flush
            }
            // ! Straight Flush?
            for (int i = flushCards.Count -  5; i >= 0; i--)
            {
                List<Card> temp5 = flushCards.GetRange(i, 5);
                if(debugEnable)Console.WriteLine();
                if(HasConsecutiveValue(temp5) && ContainsPlayerCard(temp5)){
                    WinningHand tempWinning = new(HandType.StraightFlush, temp5);
                    player.WinningHands.Add(tempWinning);
                    if(debugEnable){
                        Console.Write($"\n{flushCards.Count} Card Flush - HIGHEST STRAIGHT FLUSH: ");
                        foreach (Card c in temp5)
                        {
                            Console.Write($"{c} ");
                        }
                    }
                    return;
                }
            }


            // ! If not Royal Flush or Straight Flush. It's just a regular Flush
            for (int index = 0; index < flushCards.Count; index++)
            {
                if (flushCards[index].Value == 1)
                {
                    flushCards[index].Value = 14;
                }
            }

            flushCards = flushCards.OrderBy(c => c.Value).ToList();

            for (int i = flushCards.Count - 5; i >= 0; i--)
            {
                List<Card> temp5 = flushCards.GetRange(i, 5);
                if (ContainsPlayerCard(temp5))
                {
                    WinningHand tempWinning = new(HandType.Flush, temp5);
                    player.WinningHands.Add(tempWinning);
                    if(debugEnable){
                        Console.Write($"{flushCards.Count} Card Flush - HIGHEST FLUSH: ");
                        foreach (Card c in temp5)
                        {
                            Console.Write($"{c} ");
                        }
                    }
                    return;
                }
            }

            // ! For Testing
            if(debugEnable) Console.WriteLine("-NO FLUSH WITH PLAYER CARDS-");
            if(testingEnable){
                WinningHand tempWinningHand = new(HandType.Nothing, new List<Card>());
                player.WinningHands.Add(tempWinningHand);
            }
        }
    
        private static bool ContainsPlayerCard(List<Card> cards){
            foreach (Card c in cards)
            {
                if(c.IsPlayerCard){
                    return true;
                }
            }
            return false;
        }

        private static bool HasConsecutiveValue(List<Card> cards)
        {
            int startingValue = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (i == 0)
                {
                    startingValue = cards[i].Value;
                }
                else
                {
                    if (cards[i].Value != startingValue + 1)
                    {
                        return false;
                    }
                    startingValue++;
                }
            }
            return true;
        }

    }
}
