namespace PokerAlgo
{
    static class Algo
    {
        private static bool debugEnable = false;
        public static bool unitTestingEnable = false;

        public static void FindWinner(List<Player> players, List<Card> community)
        {
            // Lot of fancy code stuff
            foreach (Player player in players)
            {
                DeterminePlayerHands(player, community);
                Console.WriteLine(player.Name + ":");
                foreach (WinningHand hand in player.WinningHands)
                {
                    Console.WriteLine("   " + hand);
                }
            }

            // * Manual Testing
            // Player testPlayer = new Player("Test",
            // new Card(10, CardSuit.Diamonds, true),
            // new Card(11, CardSuit.Diamonds, true));

            // List<Card> testCom = new List<Card>(){
            // new Card(9, CardSuit.Diamonds, false),
            // new Card(8, CardSuit.Diamonds, false),
            // new Card(13, CardSuit.Diamonds, false),
            // new Card(12, CardSuit.Diamonds, false),
            // new Card(14, CardSuit.Diamonds, false)};

            // DeterminePlayerHands(testPlayer, testCom);
        }

        // * Main Methods
        private static void DeterminePlayerHands(Player player, List<Card> community)
        {
            // * Combine and sort cards
            List<Card> cards = new()
            {
                player.Hand.Item1,
                player.Hand.Item2
            };
            cards.AddRange(community);

            SortCardsByValue(cards);

            DebugLogCards("All Cards", cards);

            AddLowAces(cards);

            FlushFinder(cards, player);
            StraightFinder(cards, player);
            MultipleFinder(cards, player);
        }


        public static void FlushFinder(List<Card> combinedCards, Player player)
        {
            List<Card> flushCards = combinedCards.GroupBy(card => card.Suit)
            .Where(group => group.Count() >= 5)
            .SelectMany(group => group).ToList();

            if (flushCards.Count == 0)
            {
                if(debugEnable) Console.WriteLine("- No Flush -");
                TestingAddNoWinningHand(player);
                return;
            }

            DebugLogCards("Flush Finder - Flush Cards", flushCards);

            List<Card> bestFive = new();

            // ! Royal Flush
            bestFive = flushCards.GetRange(flushCards.Count - 5, 5);
            if(bestFive.ElementAt(0).Value == 10 && HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive)){
                AddWinningHand(player, HandType.RoyalFlush, bestFive);
                return;
            }

            // ! Straight Flush
            for (int i = flushCards.Count - 5; i >= 0; i--)
            {
                bestFive = flushCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive))
                {
                    AddWinningHand(player, HandType.StraightFlush, bestFive);
                    return;
                }
            }

            // ! Standard Flush
            for (int i = flushCards.Count - 5; i >= 0; i--)
            {
                bestFive = flushCards.GetRange(i, 5);
                if (HasPlayerCard(bestFive))
                {
                    AddWinningHand(player, HandType.Flush, bestFive);
                    return;
                }
            }

            if(debugEnable) Console.WriteLine("\n- No Flush -");
            TestingAddNoWinningHand(player);
        }
    

        public static void StraightFinder(List<Card> cards, Player player){
            List<Card> tempCards = new();
            // ! Deep Copy cards
            foreach (Card c in cards)
            {
                tempCards.Add(new Card(c));
            }

            DebugLogCards("Straight Finder", tempCards);

            // ! Removes duplicates
            for (int i = tempCards.Count - 1; i > 0; i--)
            {
                if(tempCards[i].Value == tempCards[i-1].Value){
                    if(tempCards[i].IsPlayerCard && tempCards[i - 1].IsPlayerCard || !tempCards[i].IsPlayerCard && !tempCards[i - 1].IsPlayerCard)
                    {
                        tempCards.RemoveAt(i);
                    }
                    else if(tempCards[i].IsPlayerCard){
                        tempCards.RemoveAt(i - 1);
                    }
                    else{
                        tempCards.RemoveAt(i);
                    }
                }
            }

            for (int i = tempCards.Count - 5; i >= 0; i--)
            {
                List<Card> bestFive = tempCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive) && !IsSameSuit(bestFive))
                {
                    AddWinningHand(player, HandType.Straight, bestFive);
                    return;
                }
            }

            // ! For Testing
            if (debugEnable) Console.WriteLine("- No Straight -");
            TestingAddNoWinningHand(player);
        }


        public static void MultipleFinder(List<Card> cards, Player player){
            List<Card> duplicateCards = RemoveLowAces(cards);
            duplicateCards = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group).ToList();

            if(duplicateCards.Count == 0){
                TestingAddNoWinningHand(player);
                return;
            }

            DebugLogCards("Multiple Finder - Duplicates", duplicateCards);

            // ! Four of a Kind
            List<Card> fourKind = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 4).SelectMany(group => group).ToList();

            if(fourKind.Count == 4 && HasPlayerCard(duplicateCards)){
                AddWinningHand(player, HandType.FourKind, fourKind);
                return;
            }

            List<Card> threeKinds = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 3)
            .SelectMany(group => group).ToList();

            List<Card> pairs = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 2)
            .SelectMany(group => group).ToList();

            SortCardsByValue(threeKinds);
            SortCardsByValue(pairs);

            // * Possible Combinations
            // * 7 cards. 2x 3k | 1x 3k , 2x pair | 1x 3k , 1x pair | 3k | 3x pair | 2x pair | 1x pair

            // ! Two, 3 of a kind. Full House
            if (threeKinds.Count == 6){                
                List<Card> top3 = threeKinds.GetRange(3, 3);
                List<Card> bottom3 = threeKinds.GetRange(0, 3);

                List<Card> fullHouse = new();
                if(HasPlayerCard(top3) || HasPlayerCard(bottom3))
                {
                    fullHouse.AddRange(top3);
                    if(bottom3[0].IsPlayerCard && bottom3[1].IsPlayerCard){
                        fullHouse.AddRange(bottom3.GetRange(0, 2));
                    }
                    else if(bottom3[1].IsPlayerCard && bottom3[2].IsPlayerCard){
                        fullHouse.AddRange(bottom3.GetRange(1, 2));
                    }
                    else{
                        fullHouse.AddRange(bottom3.GetRange(1, 2));
                    }
                }
                else{
                    throw new Exception("MultipleFinder() 2x, 3 of a kind. Something went very wrong. It's not possible for 2, 3Ks to exist and the player not have a card in.");
                }

                AddWinningHand(player, HandType.FullHouse, fullHouse);
            }
           
            // ! One, 3 of a kind and 2 pairs. Full House
            else if(threeKinds.Count == 3 && pairs.Count == 4){
                List<Card> topPair = pairs.GetRange(2, 2);
                List<Card> bottomPair = pairs.GetRange(0, 2);
                List<Card> fullHouse = new();

                if (HasPlayerCard(threeKinds) || HasPlayerCard(topPair))
                {
                    fullHouse.AddRange(threeKinds);
                    fullHouse.AddRange(topPair);
                }
                else if(HasPlayerCard(bottomPair)){
                    fullHouse.AddRange(threeKinds);
                    fullHouse.AddRange(bottomPair);
                }
                else
                {
                    throw new Exception("MultipleFinder() 1x, 3 of a kind, 2 pairs. Something went very wrong. It's not possible for 1x 3K and 2 pairs to exist and the player not have a card in.");
                }

                AddWinningHand(player, HandType.FullHouse, fullHouse);
            }
          
            // ! One,  3 of a kind and 1 pair. Full House
            else if(threeKinds.Count == 3 && pairs.Count == 2){
                List<Card> fullHouse = new();
                fullHouse.AddRange(threeKinds);
                fullHouse.AddRange(pairs);

                if(HasPlayerCard(fullHouse)){
                    AddWinningHand(player, HandType.FullHouse, fullHouse);
                }
                else
                {
                    TestingAddNoWinningHand(player);
                }
            }

            // ! Three of a kind
            else if (threeKinds.Count == 3)
            {
                if (HasPlayerCard(threeKinds))
                {
                    AddWinningHand(player, HandType.ThreeKind, threeKinds);
                }
                else
                {
                    TestingAddNoWinningHand(player);
                }
            }

            // ! 3 Pairs
            else if (pairs.Count == 6){
                List<Card> topPair = pairs.GetRange(4,2);
                List<Card> midPair = pairs.GetRange(2,2);
                List<Card> bottomPair = pairs.GetRange(0,2);

                List<Card> twoPairs = new();
                twoPairs.AddRange(topPair);

                if (HasPlayerCard(midPair)){
                    twoPairs.AddRange(midPair);
                }
                else if(HasPlayerCard(bottomPair)){
                    twoPairs.AddRange(bottomPair);
                }
                else if(HasPlayerCard(topPair)){
                    twoPairs.AddRange(midPair);
                }
                else
                {
                    throw new Exception("MultipleFinder() 3 pairs. Something went very wrong. It's not possible for 3 pairs to exist and the player not have a card in.");
                }

                AddWinningHand(player, HandType.TwoPairs, twoPairs);
            }
            
            // ! 2 Pairs
            else if(pairs.Count == 4){
                pairs = pairs.OrderByDescending(x => x.Value).ToList();
                if (HasPlayerCard(pairs)){
                    AddWinningHand(player, HandType.TwoPairs, pairs);
                }
                else
                {
                    TestingAddNoWinningHand(player);
                }
            }
            
            // ! 1 Pair
            else if(pairs.Count == 2){
                if(HasPlayerCard(pairs)){
                    AddWinningHand(player, HandType.Pair, pairs);
                }
                else
                {
                    TestingAddNoWinningHand(player);
                }
            }

            // ! Nothing
            else{
                if (debugEnable) Console.WriteLine("- No Full House, 3K, 2 Pairs, or Pair -");
                TestingAddNoWinningHand(player);
            }
        }


        // * Helper Methods
        private static void AddLowAces(List<Card> cards){
            List<Card> acesToAdd = new();
            foreach (Card c in cards)
            {
                if(c.Value == 14){
                    acesToAdd.Add(new Card(1, c.Suit, c.IsPlayerCard));
                }
            }
            cards.InsertRange(0, acesToAdd);
        }

        private static List<Card> RemoveLowAces(List<Card> cards){
            return cards.Where(c => c.Value != 1).ToList();
        }

        private static void TestingAddNoWinningHand(Player player){
            if (unitTestingEnable)
            {
                player.WinningHands.Add(new(HandType.Nothing, new List<Card>()));
            }
        }

        private static void DebugLogCards(string description, List<Card> cards){
            if (debugEnable)
            {
                Console.WriteLine($"{description}: ");
                foreach (Card c in cards)
                {
                    // Console.BackgroundColor = ConsoleColor.White;
                    // Console.ForegroundColor = c.Suit == CardSuit.Spades || c.Suit == CardSuit.Clubs ? ConsoleColor.Black : ConsoleColor.Red;
                    Console.WriteLine($"{c}");
                    // Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        private static void SortCardsByValue(List<Card> cards){
            cards.Sort((x, y) => x.Value.CompareTo(y.Value));
        }

        private static bool HasPlayerCard(List<Card> cards){
            foreach (Card c in cards)
            {
                if(c.IsPlayerCard){
                    return true;
                }
            }
            return false;
        }

        private static bool HasConsecutiveValues(List<Card> cards)
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

        private static bool IsSameSuit(List<Card> cards){
            CardSuit suit = cards.ElementAt(0).Suit;
            foreach (Card c in cards)
            {
                if(c.Suit != suit){
                    return false;;
                }
            }
            return true;
        }

        private static void AddWinningHand(Player player, HandType handType, List<Card> cards)
        {
            if (HasPlayerCard(cards))
            {
                player.WinningHands.Add(new WinningHand(handType, cards));
                if (debugEnable)
                {
                    Console.Write($"{handType}: ");
                    foreach (Card c in cards)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = c.Suit == CardSuit.Spades || c.Suit == CardSuit.Clubs ? ConsoleColor.Black : ConsoleColor.Red;
                        Console.Write($"{c} ");
                        Console.ResetColor();
                    }
                }
            }
        }

    }
}
