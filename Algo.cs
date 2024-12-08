namespace PokerAlgo
{
    static class Algo
    {
        private static bool debugEnable = false;
        private static bool winnersDebug = true;
        public static bool unitTestingEnable = false;

        public static void FindWinner(List<Player> players, List<Card> communityCards)
        {
            // Lot of fancy code stuff
            foreach (Player player in players)
            {
                DeterminePlayerHands(player, communityCards);
                Console.WriteLine(player.Name + ":");
                foreach (WinningHand hand in player.WinningHands)
                {
                    Console.WriteLine("    " + hand);
                }
            }

            Player communityPlayer = DetermineCommunityHands(communityCards);
            communityPlayer.SortWinningHands();
            Tuple<string, List<Player>> winnersTuple = DetermineWinners(players, communityPlayer);

            Console.WriteLine($"\n{winnersTuple.Item1}");
            foreach (Player player in winnersTuple.Item2)
            {
                Console.WriteLine($"{player.Name}: {player.WinningHands.ElementAt(0)}");
                if(player.WinningHands.ElementAt(0).Type == HandType.Nothing){
                    Console.WriteLine(player.Hand);
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

            FindFlush(cards, player);
            FindStraight(cards, player);
            FindMultiple(cards, player);
            player.SortWinningHands();
            player.SortHand();
            if(player.WinningHands.Count == 0){
                player.WinningHands.Add(new WinningHand(HandType.Nothing, new List<Card>()));
            }
        }


        public static void FindFlush(List<Card> combinedCards, Player player)
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

            DebugLogCards("FindFlush - Flush Cards", flushCards);

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
            flushCards = RemoveLowAces(flushCards);
            if(flushCards.Count >= 5){
                for (int i = flushCards.Count - 5; i >= 0; i--)
                {
                    bestFive = flushCards.GetRange(i, 5);
                    if (HasPlayerCard(bestFive))
                    {
                        AddWinningHand(player, HandType.Flush, bestFive);
                        return;
                    }
                }
            }

            if(debugEnable) Console.WriteLine("\n- No Flush -");
            TestingAddNoWinningHand(player);
        }
    
        public static void FindStraight(List<Card> cards, Player player){
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

        public static void FindMultiple(List<Card> cards, Player player){
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
                twoPairs.AddRange(topPair);

                AddWinningHand(player, HandType.TwoPairs, twoPairs);
            }
            
            // ! 2 Pairs
            else if(pairs.Count == 4){
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


        public static Tuple<string, List<Player>> DetermineWinners(List<Player> players, Player community){
            List<Player> sortedPlayers = players.OrderBy(p => p.WinningHands.ElementAt(0).Type).ToList();
            List<Player> winners = new();

            if (community.WinningHands.ElementAt(0).Type == HandType.RoyalFlush)
            {
                return new Tuple<string, List<Player>>("Royal Flush", sortedPlayers);
            }
            else if (community.WinningHands.ElementAt(0).Type == HandType.StraightFlush ||
            community.WinningHands.ElementAt(0).Type == HandType.FullHouse ||
            community.WinningHands.ElementAt(0).Type == HandType.Flush ||
            community.WinningHands.ElementAt(0).Type == HandType.Straight)
            {
                sortedPlayers.Add(community);
                sortedPlayers = sortedPlayers.OrderBy(p => p.WinningHands.ElementAt(0).Type).ToList();
            }

            // ! We compare player's best hand type
            for (int i = sortedPlayers.Count - 1; i > 0; i--)
            {
                if(sortedPlayers[i].WinningHands.ElementAt(0).Type > sortedPlayers[i-1].WinningHands.ElementAt(0).Type){
                    winners.Add(sortedPlayers[i]);
                    break;
                }
                else if(sortedPlayers[i].WinningHands.ElementAt(0).Type == sortedPlayers[i - 1].WinningHands.ElementAt(0).Type)
                {
                    winners.Add(sortedPlayers[i]);
                    if(i==1){
                        winners.Add(sortedPlayers[0]);
                    }
                }
            }

            if(winnersDebug)
            {
                Console.WriteLine("\nWinners:");
                foreach (Player player in winners)
                {
                    Console.WriteLine($"\t{player.Name}: {player.WinningHands.ElementAt(0)}");
                    if (player.WinningHands.ElementAt(0).Type == HandType.Nothing)
                    {
                        Console.Write(player.Hand);
                    }
                }
            }

            if(winners.Count > 1){
                return BreakTie(winners, community, sortedPlayers);
            }
            else if(winners.Count == 1){
                if(winners.ElementAt(0).Name == "Community"){
                    return new Tuple<string, List<Player>>("Community Wins, Tie", sortedPlayers);
                }
                else{
                    return new Tuple<string, List<Player>>("Simple Player Win", winners);
                }
            }

            throw new Exception($"DetermineWinners: Something went very wrong. winners.Count={winners.Count}");
        }

        private static Tuple<string, List<Player>> BreakTie(List<Player> tieWinners, Player communityPlayer, List<Player> allPlayers){
            // ! Logic if there is a tie
            HandType tieType = tieWinners.ElementAt(0)
            .WinningHands.ElementAt(0).Type;

            List<Player> tempWinners = tieWinners.ToList();

            switch (tieType)
            {
                case HandType.RoyalFlush:
                    return new Tuple<string, List<Player>>("Player Royal Flush Tie", tieWinners);
                case HandType.StraightFlush:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        List<Card> current = tieWinners[playerIndex].WinningHands.ElementAt(0).Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHands.ElementAt(0).Cards;
                        for (int i = 4; i >= 0; i--){
                            if (current[i].Value > next[i].Value){
                                tempWinners.Remove(tieWinners[playerIndex + 1]);
                                break;
                            }
                            else if(current[i].Value < next[i].Value){
                                tempWinners.Remove(tieWinners[playerIndex]);
                                break;
                            }
                        }
                    }

                    if(tempWinners.Count == 1){
                        if(tempWinners.Contains(communityPlayer)){
                            return new Tuple<string, List<Player>>("Straight Flush Tie By Community", allPlayers);
                        }
                        return new Tuple<string, List<Player>>("Straight Flush Simple Win", tempWinners);
                    }
                    else{
                        if (tempWinners.Contains(communityPlayer))
                        {
                            return new Tuple<string, List<Player>>("Straight Flush Tie By Community", allPlayers);
                        }
                        return new Tuple<string, List<Player>>("Straight Flush Tie", tempWinners);
                    }
                
                case HandType.FourKind:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        List<Card> current = tieWinners[playerIndex].WinningHands.ElementAt(0).Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHands.ElementAt(0).Cards;
                        if (current[0].Value > next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current[0].Value < next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                    }
                    if (tempWinners.Count == 1)
                    {
                        return new Tuple<string, List<Player>>("Player FourKind Simple Win", tempWinners);
                    }
                    else
                    {
                        return new Tuple<string, List<Player>>("Player FourKind Tie", tempWinners);
                    }
                
                case HandType.FullHouse:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        List<Card> current = tieWinners[playerIndex].WinningHands.ElementAt(0).Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHands.ElementAt(0).Cards;
                        if (current[0].Value > next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current[0].Value < next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                        else if (current[3].Value > next[3].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current[3].Value < next[3].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                    }
                    if (tempWinners.Count == 1)
                    {
                        if (tempWinners.Contains(communityPlayer))
                        {
                            return new Tuple<string, List<Player>>("Full House Tie By Community", allPlayers);
                        }
                        return new Tuple<string, List<Player>>("Full House Simple Win", tempWinners);
                    }
                    else
                    {
                        if (tempWinners.Contains(communityPlayer))
                        {
                            return new Tuple<string, List<Player>>("Full House Tie By Community", allPlayers);
                        }
                        return new Tuple<string, List<Player>>("Full House Tie", tempWinners);
                    }
                
                case HandType.Flush:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        List<Card> current = tieWinners[playerIndex].WinningHands.ElementAt(0).Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHands.ElementAt(0).Cards;
                        for (int i = 4; i >= 0; i--)
                            {
                            if (current[i].Value > next[i].Value)
                            {
                                tempWinners.Remove(tieWinners[playerIndex + 1]);
                                break;
                            }
                            else if (current[i].Value < next[i].Value)
                            {
                                tempWinners.Remove(tieWinners[playerIndex]);
                                break;
                            }
                        }
                    }

                    if (tempWinners.Count == 1)
                    {
                        if (tempWinners.Contains(communityPlayer))
                        {
                            return new Tuple<string, List<Player>>("Flush Tie By Community", allPlayers);
                        }
                        return new Tuple<string, List<Player>>("Flush Simple Win", tempWinners);
                    }
                    else
                    {
                        if (tempWinners.Contains(communityPlayer))
                        {
                            return new Tuple<string, List<Player>>("Flush Tie By Community", allPlayers);
                        }
                        return new Tuple<string, List<Player>>("Flush Tie", tempWinners);
                    }

                case HandType.Straight:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        List<Card> current = tieWinners[playerIndex].WinningHands.ElementAt(0).Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHands.ElementAt(0).Cards;
                        for (int i = 4; i >= 0; i--)
                        {
                            if (current[i].Value > next[i].Value)
                            {
                                tempWinners.Remove(tieWinners[playerIndex + 1]);
                                break;
                            }
                            else if (current[i].Value < next[i].Value)
                            {
                                tempWinners.Remove(tieWinners[playerIndex]);
                                break;
                            }
                        }
                    }

                    if (tempWinners.Count == 1)
                    {
                        if (tempWinners.Contains(communityPlayer))
                        {
                            return new Tuple<string, List<Player>>("Straight Tie By Community", allPlayers);
                        }
                        return new Tuple<string, List<Player>>("Straight Simple Win", tempWinners);
                    }
                    else
                    {
                        if (tempWinners.Contains(communityPlayer))
                        {
                            return new Tuple<string, List<Player>>("Straight Tie By Community", allPlayers);
                        }
                        return new Tuple<string, List<Player>>("Straight Tie", tempWinners);
                    }

                case HandType.ThreeKind:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        List<Card> current = tieWinners[playerIndex].WinningHands.ElementAt(0).Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHands.ElementAt(0).Cards;
                        if (current[0].Value > next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current[0].Value < next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                    }
                    if (tempWinners.Count == 1)
                    {
                        return new Tuple<string, List<Player>>("3Kind Simple Win", tempWinners);
                    }
                    else
                    {
                        return new Tuple<string, List<Player>>("3Kind Tie", tempWinners);
                    }

                case HandType.TwoPairs:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        List<Card> current = tieWinners[playerIndex].WinningHands.ElementAt(0).Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHands.ElementAt(0).Cards;
                        if (current[2].Value > next[2].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current[2].Value < next[2].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                        else if (current[0].Value > next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current[0].Value < next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                    }
                    if (tempWinners.Count == 1)
                    {
                        return new Tuple<string, List<Player>>("2Pairs Simple Win", tempWinners);
                    }
                    else
                    {
                        return new Tuple<string, List<Player>>("2Pairs Tie", tempWinners);
                    }

                case HandType.Pair:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        List<Card> current = tieWinners[playerIndex].WinningHands.ElementAt(0).Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHands.ElementAt(0).Cards;
                        if (current[0].Value > next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current[0].Value < next[0].Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                    }
                    if (tempWinners.Count == 1)
                    {
                        return new Tuple<string, List<Player>>("Pair Simple Win", tempWinners);
                    }
                    else
                    {
                        return new Tuple<string, List<Player>>("Pair Tie", tempWinners);
                    }

                case HandType.Nothing:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        Tuple<Card, Card> current = tieWinners[playerIndex].Hand;
                        Tuple<Card, Card> next = tieWinners[playerIndex + 1].Hand;
                        if (current.Item2.Value > next.Item2.Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current.Item2.Value < next.Item2.Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                        else if (current.Item1.Value > next.Item1.Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current.Item1.Value < next.Item1.Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                    }
                    if (tempWinners.Count == 1)
                    {
                        return new Tuple<string, List<Player>>("High Card Simple Win", tempWinners);
                    }
                    else
                    {
                        return new Tuple<string, List<Player>>("High Card Tie", tempWinners);
                    }

                default:
                    throw new Exception("BreakTie: Something went very wrong");
            }
        }

        private static Player DetermineCommunityHands(List<Card> communityCards){
            // ! Detect Royal Flush, Straight Flush, Full House, Flush, and Straight in community.
            List<Card> cards = communityCards.OrderBy(c => c.Value).ToList();
            Player community = new ("Community", new Card(0, CardSuit.Spades, true), new Card(0, CardSuit.Spades, true));

            AddLowAces(cards);

            FindCommunityFlush(cards, community);
            FindCommunityStraight(cards, community);
            FindCommunityFullHouse(cards, community);

            if(community.WinningHands.Count == 0){
                community.WinningHands.Add(new WinningHand(HandType.Nothing, new List<Card>()));
            }

            return community;
        }

        private static void FindCommunityFlush(List<Card> cards, Player community)
        {
            List<Card> flushCards = cards.GroupBy(card => card.Suit)
                        .Where(group => group.Count() >= 5)
                        .SelectMany(group => group).ToList();

            if (flushCards.Count == 0)
            {
                if (debugEnable) Console.WriteLine("- FindCommunityFlush No Flush -");
                TestingAddNoWinningHand(community);
                return;
            }

            DebugLogCards("FindCommunityFlush - Flush Cards", flushCards);

            List<Card> bestFive = new();

            // ! Royal Flush
            bestFive = flushCards.GetRange(flushCards.Count - 5, 5);
            if (bestFive.ElementAt(0).Value == 10 && HasConsecutiveValues(bestFive))
            {
                AddWinningHand(community, HandType.RoyalFlush, bestFive);
                return;
            }

            // ! Straight Flush
            for (int i = flushCards.Count - 5; i >= 0; i--)
            {
                bestFive = flushCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive))
                {
                    AddWinningHand(community, HandType.StraightFlush, bestFive);
                    return;
                }
            }

            // ! Standard Flush
            flushCards = RemoveLowAces(flushCards);
            if (flushCards.Count == 5)
            {
                AddWinningHand(community, HandType.Flush, flushCards);
                return;
            }

            if (debugEnable) Console.WriteLine("\n- FindCommunityFlush No Flush -");
            TestingAddNoWinningHand(community);
        }

        private static void FindCommunityStraight(List<Card> cards, Player community){
            List<Card> tempCards = new();
            // ! Deep Copy cards
            foreach (Card c in cards)
            {
                tempCards.Add(new Card(c));
            }

            DebugLogCards("StraightCommunityFinder", tempCards);

            // ! Removes duplicates
            for (int i = tempCards.Count - 1; i > 0; i--)
            {
                if (tempCards[i].Value == tempCards[i - 1].Value)
                {
                    tempCards.RemoveAt(i);
                }
            }

            for (int i = tempCards.Count - 5; i >= 0; i--)
            {
                List<Card> bestFive = tempCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive) && !IsSameSuit(bestFive))
                {
                    AddWinningHand(community, HandType.Straight, bestFive);
                    return;
                }
            }

            // ! For Testing
            if (debugEnable) Console.WriteLine("- FindCommunityStraight - No Straight -");
            TestingAddNoWinningHand(community);

        }

        private static void FindCommunityFullHouse(List<Card> cards, Player community){
            List<Card> duplicateCards = RemoveLowAces(cards);

            duplicateCards = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group).ToList();

            DebugLogCards("FindCommunityFullHouse - Duplicates", duplicateCards);

            List<Card> threeKinds = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 3)
            .SelectMany(group => group).ToList();

            List<Card> pairs = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 2)
            .SelectMany(group => group).ToList();

            SortCardsByValue(threeKinds);
            SortCardsByValue(pairs);

            // ! One,  3 of a kind and 1 pair. Full House
            if (threeKinds.Count == 3 && pairs.Count == 2)
            {
                List<Card> fullHouse = new();
                fullHouse.AddRange(threeKinds);
                fullHouse.AddRange(pairs);

                AddWinningHand(community, HandType.FullHouse, fullHouse);
            }

        }

        // * Helper Methods

        private static void AddWinningHand(Player player, HandType handType, List<Card> cards)
        {
            if(player.Name == "Community"){
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
            else if (HasPlayerCard(cards))
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

    }
}
