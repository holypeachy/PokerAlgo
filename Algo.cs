namespace PokerAlgo
{
    static class Algo
    {
        private static bool debugEnable = false;
        public static bool unitTestingEnable = true;

        public static void FindWinner(List<Player> players, List<Card> community)
        {
            // Lot of fancy code stuff
            // DeterminePlayerHands(players[0], community);
            Player testPlayer = new Player("Test",
            new Card(1, CardSuit.Hearts, true),
            new Card(7, CardSuit.Spades, true));

            List<Card> testCom = new List<Card>(){
            new Card(2, CardSuit.Diamonds, false),
            new Card(2, CardSuit.Clubs, false),
            new Card(2, CardSuit.Hearts, false),
            new Card(5, CardSuit.Clubs, false),
            new Card(5, CardSuit.Diamonds, false)};

            DeterminePlayerHands(testPlayer, testCom);
        }

        // * Main Methods
        private static void DeterminePlayerHands(Player player, List<Card> community)
        {
            // ! Combine and sort cards
            List<Card> cards = new();

            cards.Add(new Card(player.Hand.Item1));
            cards.Add(new Card(player.Hand.Item2));
            foreach (Card c in community)
            {
                cards.Add(c);
            }

            cards = cards.OrderBy(x => x.Value).ToList();

            if(debugEnable){
                Console.WriteLine("All Cards: ");
                foreach (Card c in cards)
                {
                    Console.WriteLine($"{c}" + (c.IsPlayerCard ? "player" : ""));
                }
                Console.WriteLine();
            }

            MultipleFinder(cards, player);
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
                if(unitTestingEnable){
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

            // ! Do we have a royal flush?
            bool isRoyalFlush = true;
            bool[] royalMatches = { false, false, false, false, false };

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
            if(unitTestingEnable){
                WinningHand tempWinningHand = new(HandType.Nothing, new List<Card>());
                player.WinningHands.Add(tempWinningHand);
            }
        }
    
        public static void StraightFinder(List<Card> cards, Player player){
            List<Card> dupAces = new();
            foreach (Card c in cards)
            {
                dupAces.Add(new Card(c));
                if(c.Value == 1){
                    dupAces.Add(new Card(14, c.Suit, c.IsPlayerCard));
                }
            }
            dupAces = dupAces.OrderBy(x => x.Value).ToList();

            // ! Removes duplicates
            for (int i = dupAces.Count - 1; i > 0; i--)
            {
                if(dupAces[i].Value == dupAces[i-1].Value){
                    if(dupAces[i].IsPlayerCard && dupAces[i - 1].IsPlayerCard || !dupAces[i].IsPlayerCard && !dupAces[i - 1].IsPlayerCard)
                    {
                        dupAces.RemoveAt(i);
                    }
                    else if(dupAces[i].IsPlayerCard){
                        dupAces.RemoveAt(i - 1);
                    }
                    else{
                        dupAces.RemoveAt(i);
                    }
                }
            }

            for (int i = dupAces.Count - 5; i >= 0; i--)
            {
                List<Card> temp5 = dupAces.GetRange(i, 5);
                if (debugEnable) Console.WriteLine();
                if (HasConsecutiveValue(temp5) && ContainsPlayerCard(temp5) && !AllSameSuit(temp5))
                {
                    WinningHand tempWinning = new(HandType.Straight, temp5);
                    player.WinningHands.Add(tempWinning);
                    if (debugEnable)
                    {
                        Console.Write($"Straight - HIGHEST STRAIGHT: ");
                        foreach (Card c in temp5)
                        {
                            Console.Write($"{c} ");
                        }
                    }
                    return;
                }
            }

            // ! For Testing
            if (debugEnable) Console.WriteLine("-NO STRAIGHT-");
            if (unitTestingEnable)
            {
                WinningHand tempWinningHand = new(HandType.Nothing, new List<Card>());
                player.WinningHands.Add(tempWinningHand);
            }
        }

        public static void MultipleFinder(List<Card> cards, Player player){
            List<Card> duplicateCards = cards.GroupBy(card => card.Value)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group).ToList();

            // Make Aces value into 14
            for (int i = 0; i < duplicateCards.Count; i++)
            {
                if (duplicateCards[i].Value == 1)
                {
                    duplicateCards[i] = new Card(14, duplicateCards[i].Suit, duplicateCards[i].IsPlayerCard);
                }
            }

            duplicateCards = duplicateCards.OrderBy(x => x.Value).ToList();

            if(debugEnable){
                Console.WriteLine("Multiple Finder - Duplicates:");
                foreach (Card c in duplicateCards)
                {
                    Console.WriteLine(c);
                }
                Console.WriteLine();
            }

            // ! Four of a Kind
            List<Card> fourKind = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 4).SelectMany(group => group).ToList();

            if(fourKind.Count == 4 && ContainsPlayerCard(duplicateCards)){
                WinningHand tempWinning = new(HandType.FourKind, fourKind);
                player.WinningHands.Add(tempWinning);
                if (debugEnable)
                {
                    Console.Write($"FOUR OF A KIND: ");
                    foreach (Card c in fourKind)
                    {
                        Console.Write($"{c} ");
                    }
                }
            }

            List<Card> threeKinds = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 3).SelectMany(group => group).ToList();
            List<Card> pairs = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 2).SelectMany(group => group).ToList();

            threeKinds = threeKinds.OrderBy(x => x.Value).ToList();
            pairs = pairs.OrderBy(x => x.Value).ToList();

            // * Quick Notes. Possible Combinations
            // * 7 cards. 2x 3k | 1x 3k , 2x pair | 1x 3k , 1x pair | 3x pair | 2x pair | 1x pair

            // ! Two, 3 of a kind. Full House
            if (threeKinds.Count == 6){                
                List<Card> top3 = threeKinds.GetRange(3, 3);
                List<Card> bottom3 = threeKinds.GetRange(0, 3);

                List<Card> fullHouse = new();
                if(ContainsPlayerCard(top3) || ContainsPlayerCard(bottom3))
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

                WinningHand tempWinning = new(HandType.FullHouse, fullHouse);
                player.WinningHands.Add(tempWinning);
                if (debugEnable)
                {
                    Console.Write($"2x 3K - FULL HOUSE: ");
                    foreach (Card c in fullHouse)
                    {
                        Console.Write($"{c} ");
                    }
                    Console.WriteLine();
                }
            }
           
            // ! One, 3 of a kind and 2 pairs. Full House
            else if(threeKinds.Count == 3 && pairs.Count == 4){
                List<Card> topPair = pairs.GetRange(2, 2);
                List<Card> bottomPair = pairs.GetRange(0, 2);
                List<Card> fullHouse = new();

                if (ContainsPlayerCard(threeKinds) || ContainsPlayerCard(topPair))
                {
                    fullHouse.AddRange(threeKinds);
                    fullHouse.AddRange(topPair);
                }
                else if(ContainsPlayerCard(bottomPair)){
                    fullHouse.AddRange(threeKinds);
                    fullHouse.AddRange(bottomPair);
                }
                else
                {
                    throw new Exception("MultipleFinder() 1x, 3 of a kind, 2 pairs. Something went very wrong. It's not possible for 1x 3K and 2 pairs to exist and the player not have a card in.");
                }

                WinningHand tempWinning = new(HandType.FullHouse, fullHouse);
                player.WinningHands.Add(tempWinning);
                if (debugEnable)
                {
                    Console.Write($"3K, 2 Pairs - FULL HOUSE: ");
                    foreach (Card c in fullHouse)
                    {
                        Console.Write($"{c} ");
                    }
                    Console.WriteLine();
                }
            }
          
            // ! One,  3 of a kind and 1 pair. Full House
            else if(threeKinds.Count == 3 && pairs.Count == 2){
                List<Card> fullHouse = new();
                fullHouse.AddRange(threeKinds);
                fullHouse.AddRange(pairs);

                if(ContainsPlayerCard(fullHouse)){
                    WinningHand tempWinning = new(HandType.FullHouse, fullHouse);
                    player.WinningHands.Add(tempWinning);
                    if (debugEnable)
                    {
                        Console.Write($"FULL HOUSE: ");
                        foreach (Card c in fullHouse)
                        {
                            Console.Write($"{c} ");
                        }
                        Console.WriteLine();
                    }
                }
            }
           
           // ! 3 Pairs
            else if(pairs.Count == 6){
                List<Card> topPair = pairs.GetRange(4,2);
                List<Card> midPair = pairs.GetRange(2,2);
                List<Card> bottomPair = pairs.GetRange(0,2);

                List<Card> twoPairs = new();
                twoPairs.AddRange(topPair);

                if (ContainsPlayerCard(midPair)){
                    twoPairs.AddRange(midPair);
                }
                else if(ContainsPlayerCard(bottomPair)){
                    twoPairs.AddRange(bottomPair);
                }
                else if(ContainsPlayerCard(topPair)){
                    twoPairs.AddRange(midPair);
                }
                else
                {
                    throw new Exception("MultipleFinder() 3 pairs. Something went very wrong. It's not possible for 3 pairs to exist and the player not have a card in.");
                }

                WinningHand tempWinning = new(HandType.TwoPairs, twoPairs);
                player.WinningHands.Add(tempWinning);
                if (debugEnable)
                {
                    Console.Write($"3 PAIRS - TWO PAIRS: ");
                    foreach (Card c in twoPairs)
                    {
                        Console.Write($"{c} ");
                    }
                    Console.WriteLine();
                }
            }
            
            // ! 2 Pairs
            else if(pairs.Count == 4){
                if(ContainsPlayerCard(pairs)){
                    WinningHand tempWinning = new(HandType.TwoPairs, pairs);
                    player.WinningHands.Add(tempWinning);
                    if (debugEnable)
                    {
                        Console.Write($"2 PAIRS - TWO PAIRS: ");
                        foreach (Card c in pairs)
                        {
                            Console.Write($"{c} ");
                        }
                        Console.WriteLine();
                    }
                }
            }
            
            // ! 1 Pair
            else if(pairs.Count == 2){
                if(ContainsPlayerCard(pairs)){
                    WinningHand tempWinning = new(HandType.Pair, pairs);
                    player.WinningHands.Add(tempWinning);
                    if (debugEnable)
                    {
                        Console.Write($"PAIR: ");
                        foreach (Card c in pairs)
                        {
                            Console.Write($"{c} ");
                        }
                        Console.WriteLine();
                    }
                }
            }

            // ! Nothing
            else{
                if (debugEnable) Console.WriteLine("-NO FULL HOUSE, 3K, 2 PAIRS, OR PAIR-");
                if (unitTestingEnable)
                {
                    WinningHand tempWinningHand = new(HandType.Nothing, new List<Card>());
                    player.WinningHands.Add(tempWinningHand);
                }
            }
        }


        // * Helper Methods
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

        private static bool AllSameSuit(List<Card> cards){
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
            if (ContainsPlayerCard(cards))
            {
                player.WinningHands.Add(new WinningHand(handType, cards));
                if (debugEnable)
                {
                    Console.WriteLine($"{handType}: {string.Join(", ", cards)}");
                }
            }
        }

    }
}
