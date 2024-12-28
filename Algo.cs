namespace PokerAlgo
{
    static class Algo
    {
        private static bool _debugEnable = true;
        public static bool _unitTestingEnable = false; // TODO Work in Progress

        public static void FindWinner(List<Player> players, List<Card> communityCards)
        {
            if(_debugEnable) Console.WriteLine("\n--- 🔎 Algo Starts");
            foreach (Player player in players)
            {
                if (_debugEnable)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("🃏 Determining Hands for " + player.Name + "\n");
                    Console.ResetColor();
                }
                DeterminePlayerWinningHand(player, communityCards);
            }

            // ! Temporary
            // Player communityPlayer = DetermineCommunityHands(communityCards);
            // communityPlayer.SortWinningHands();
            // Tuple<string, List<Player>> winnersTuple = DetermineWinners(players, communityPlayer);

            // Console.WriteLine($"\n{winnersTuple.Item1}");
            // foreach (Player player in winnersTuple.Item2)
            // {
            //     Console.WriteLine($"{player.Name}: {player.WinningHands.ElementAt(0)}");
            //     if(player.WinningHands.ElementAt(0).Type == HandType.Nothing){
            //         Console.WriteLine(player.Hand);
            //     }
            // }

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
        private static void DeterminePlayerWinningHand(Player player, List<Card> community)
        {
            // * Combine and sort cards
            List<Card> cards = new()
            {
                player.Hand.First,
                player.Hand.Second
            };
            cards.AddRange(community);

            SortCardsByValue(cards);

            DebugLogCards("Algo.DeterminePlayerWinningHand() - All Cards", cards);

            AddLowAces(cards);

            FindFlush(cards, player);
            if (_debugEnable) Console.WriteLine();
            FindStraight(cards, player);
            if (_debugEnable) Console.WriteLine();
            FindMultiple(cards, player);
        }

        // * Beautiful
        public static void FindFlush(List<Card> combinedCards, Player player)
        {
            List<Card> flushCards = combinedCards.GroupBy(card => card.Suit)
            .Where(group => group.Count() >= 5)
            .SelectMany(group => group).ToList();

            if (flushCards.Count == 0)
            {
                if(_debugEnable) Console.WriteLine("⚠️  Algo.FindFlush() - No Flush");
                Testing_AddNoWinningHand(player);
                return;
            }

            DebugLogCards("Algo.FindFlush() - Flush Cards", flushCards);

            List<Card> bestFive = new();

            // ! Royal Flush
            bestFive = GetBestFiveCards(flushCards);
            if(bestFive.ElementAt(0).Value == 10 && HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive)){
                SetWinningHand(player, HandType.RoyalFlush, bestFive);
                return;
            }

            // ! Straight Flush
            for (int i = flushCards.Count - 5; i >= 0; i--)
            {
                bestFive = flushCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive))
                {
                    SetWinningHand(player, HandType.StraightFlush, bestFive);
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
                        SetWinningHand(player, HandType.Flush, bestFive);
                        return;
                    }
                }
            }

            if (_debugEnable) Console.WriteLine("⚠️  Algo.FindFlush() - No Player Flush");
            Testing_AddNoWinningHand(player);
        }

        // * Beautiful
        public static void FindStraight(List<Card> cards, Player player){
            // Shallow Copy, no need to deep copy here
            List<Card> tempCards = cards.ToList();

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

            DebugLogCards("Algo.FindStraight() - Without Duplicates", tempCards);


            for (int i = tempCards.Count - 5; i >= 0; i--)
            {
                List<Card> bestFive = tempCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive) && !IsSameSuit(bestFive))
                {
                    SetWinningHand(player, HandType.Straight, bestFive);
                    return;
                }
            }

            // ! For Testing
            if (_debugEnable) Console.WriteLine("⚠️  Algo.FindStraight() - No Straight");
            Testing_AddNoWinningHand(player);
        }

        // * Okay for now, need to look at it again
        public static void FindMultiple(List<Card> cards, Player player){
            List<Card> duplicateCards = RemoveLowAces(cards);

            duplicateCards = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group).ToList();

            if(duplicateCards.Count == 0){
                if (_debugEnable) Console.WriteLine("⚠️  Algo.FindMultiple() - No Multiple");
                if (player.WinningHand == null) SetWinningHand(player, HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));
                Testing_AddNoWinningHand(player);
                return;
            }

            DebugLogCards("Algo.FindMultiple() - Duplicate Cards", duplicateCards);

            // ! Four of a Kind
            List<Card> fourKind = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 4).SelectMany(group => group).ToList();

            if(fourKind.Count == 4 && HasPlayerCard(duplicateCards)){
                SetWinningHand(player, HandType.FourKind, CompleteWinningHand(fourKind, cards));
                return;
            }
            else{
                if (_debugEnable) Console.WriteLine("⚠️  Algo.FindMultiple() - No FourKind");
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
                        fullHouse.AddRange(bottom3.GetRange(1, 2)); // ! Warning for testing, because the last 2 cards will be picked. EqualsValue???
                    }
                }
                else{
                    // ! Unecessary but should keep for now. It's 6 out of 7 cards. The player has to have at least 1.
                    throw new Exception("MultipleFinder() 2x, 3 of a kind. Something went very wrong. It's not possible for 2, 3Ks to exist and the player not have a card in.");
                }

                SetWinningHand(player, HandType.FullHouse, fullHouse);
                return;
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
                    // ! Unecessary but should keep for now. It's 7 out of 7 cards. The player has to have at least 1.
                    throw new Exception("MultipleFinder() 1x, 3 of a kind, 2 pairs. Something went very wrong. It's not possible for 1x 3K and 2 pairs to exist and the player not have a card in.");
                }

                SetWinningHand(player, HandType.FullHouse, fullHouse);
                return;
            }
          
            // ! One,  3 of a kind and 1 pair. Full House
            else if(threeKinds.Count == 3 && pairs.Count == 2){
                List<Card> fullHouse = new();
                fullHouse.AddRange(threeKinds);
                fullHouse.AddRange(pairs);

                if(HasPlayerCard(fullHouse)){
                    SetWinningHand(player, HandType.FullHouse, fullHouse);
                    return;
                }
                else
                {
                    Testing_AddNoWinningHand(player);
                }
            }

            // ! Three of a kind
            else if (threeKinds.Count == 3)
            {
                if (HasPlayerCard(threeKinds))
                {
                    SetWinningHand(player, HandType.ThreeKind, CompleteWinningHand(threeKinds, cards));
                }
                else
                {
                    Testing_AddNoWinningHand(player);
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
                    // ! Unecessary but should keep for now. It's 6 out of 7 cards. The player has to have at least 1.
                    throw new Exception("MultipleFinder() 3 pairs. Something went very wrong. It's not possible for 3 pairs to exist and the player not have a card in.");
                }
                twoPairs.AddRange(topPair);

                SetWinningHand(player, HandType.TwoPairs, CompleteWinningHand(twoPairs, cards));
            }
            
            // ! 2 Pairs
            else if(pairs.Count == 4){
                if (HasPlayerCard(pairs)){
                    SetWinningHand(player, HandType.TwoPairs, CompleteWinningHand(pairs, cards));
                }
                else
                {
                    Testing_AddNoWinningHand(player);
                }
            }
            
            // ! 1 Pair
            else if(pairs.Count == 2){
                if(HasPlayerCard(pairs)){
                    SetWinningHand(player, HandType.Pair, CompleteWinningHand(pairs,cards));
                }
                else
                {
                    Testing_AddNoWinningHand(player);
                }
            }

            // ! Nothing
            else{
                if (_debugEnable) Console.WriteLine("⚠️  Algo.FindMultiple() - No Multiple");
                if (player.WinningHand == null) SetWinningHand(player, HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));

                if (_unitTestingEnable)Testing_AddNoWinningHand(player);
            }
            
            if (player.WinningHand == null) SetWinningHand(player, HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));
        }


        // ! Second Part of Algo. Need to rewrite
        public static Tuple<string, List<Player>> DetermineWinners(List<Player> players, Player community){
            List<Player> sortedPlayers = players.OrderBy(p => p.WinningHand.Type).ToList();
            List<Player> winners = new();

            if (community.WinningHand.Type == HandType.RoyalFlush)
            {
                return new Tuple<string, List<Player>>("Royal Flush", sortedPlayers);
            }
            else if (community.WinningHand.Type == HandType.StraightFlush ||
            community.WinningHand.Type == HandType.FullHouse ||
            community.WinningHand.Type == HandType.Flush ||
            community.WinningHand.Type == HandType.Straight)
            {
                sortedPlayers.Add(community);
                sortedPlayers = sortedPlayers.OrderBy(p => p.WinningHand.Type).ToList();
            }

            // ! We compare player's best hand type
            for (int i = sortedPlayers.Count - 1; i > 0; i--)
            {
                if(sortedPlayers[i].WinningHand.Type > sortedPlayers[i-1].WinningHand.Type){
                    winners.Add(sortedPlayers[i]);
                    break;
                }
                else if(sortedPlayers[i].WinningHand.Type == sortedPlayers[i - 1].WinningHand.Type)
                {
                    winners.Add(sortedPlayers[i]);
                    if(i==1){
                        winners.Add(sortedPlayers[0]);
                    }
                }
            }

            if(_debugEnable)
            {
                Console.WriteLine("\nWinners:");
                foreach (Player player in winners)
                {
                    Console.WriteLine($"\t{player.Name}: {player.WinningHand}");
                    if (player.WinningHand.Type == HandType.Nothing)
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
            .WinningHand.Type;

            List<Player> tempWinners = tieWinners.ToList();

            switch (tieType)
            {
                case HandType.RoyalFlush:
                    return new Tuple<string, List<Player>>("Player Royal Flush Tie", tieWinners);
                case HandType.StraightFlush:
                    for (int playerIndex = 0; playerIndex < tieWinners.Count - 1; playerIndex++)
                    {
                        List<Card> current = tieWinners[playerIndex].WinningHand.Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHand.Cards;
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
                        List<Card> current = tieWinners[playerIndex].WinningHand.Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHand.Cards;
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
                        List<Card> current = tieWinners[playerIndex].WinningHand.Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHand.Cards;
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
                        List<Card> current = tieWinners[playerIndex].WinningHand.Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHand.Cards;
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
                        List<Card> current = tieWinners[playerIndex].WinningHand.Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHand.Cards;
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
                        List<Card> current = tieWinners[playerIndex].WinningHand.Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHand.Cards;
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
                        List<Card> current = tieWinners[playerIndex].WinningHand.Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHand.Cards;
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
                        List<Card> current = tieWinners[playerIndex].WinningHand.Cards;
                        List<Card> next = tieWinners[playerIndex + 1].WinningHand.Cards;
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
                        Pair<Card, Card> current = tieWinners[playerIndex].Hand;
                        Pair<Card, Card> next = tieWinners[playerIndex + 1].Hand;
                        if (current.Second.Value > next.Second.Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current.Second.Value < next.Second.Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex]);
                        }
                        else if (current.First.Value > next.First.Value)
                        {
                            tempWinners.Remove(tieWinners[playerIndex + 1]);
                        }
                        else if (current.First.Value < next.First.Value)
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

            if(community.WinningHand == null){
                community.WinningHand = (new WinningHand(HandType.Nothing, new List<Card>()));
            }

            return community;
        }


        // ! In Quarantine
        private static void FindCommunityFlush(List<Card> cards, Player community)
        {
            List<Card> flushCards = cards.GroupBy(card => card.Suit)
                        .Where(group => group.Count() >= 5)
                        .SelectMany(group => group).ToList();

            if (flushCards.Count == 0)
            {
                if (_debugEnable) Console.WriteLine("- FindCommunityFlush No Flush -");
                Testing_AddNoWinningHand(community);
                return;
            }

            DebugLogCards("FindCommunityFlush - Flush Cards", flushCards);

            List<Card> bestFive = new();

            // ! Royal Flush
            bestFive = flushCards.GetRange(flushCards.Count - 5, 5);
            if (bestFive.ElementAt(0).Value == 10 && HasConsecutiveValues(bestFive))
            {
                SetWinningHand(community, HandType.RoyalFlush, bestFive);
                return;
            }

            // ! Straight Flush
            for (int i = flushCards.Count - 5; i >= 0; i--)
            {
                bestFive = flushCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive))
                {
                    SetWinningHand(community, HandType.StraightFlush, bestFive);
                    return;
                }
            }

            // ! Standard Flush
            flushCards = RemoveLowAces(flushCards);
            if (flushCards.Count == 5)
            {
                SetWinningHand(community, HandType.Flush, flushCards);
                return;
            }

            if (_debugEnable) Console.WriteLine("\n- FindCommunityFlush No Flush -");
            Testing_AddNoWinningHand(community);
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
                    SetWinningHand(community, HandType.Straight, bestFive);
                    return;
                }
            }

            // ! For Testing
            if (_debugEnable) Console.WriteLine("- FindCommunityStraight - No Straight -");
            Testing_AddNoWinningHand(community);

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

                SetWinningHand(community, HandType.FullHouse, fullHouse);
            }

        }


        // * Helper Methods

        private static List<Card> CompleteWinningHand(List<Card> winningCards, List<Card> allCards)
        {
            List<Card> completeHand = winningCards.ToList();
            int neededNumberOfCards = 5 - winningCards.Count;
            List<Card> remainingCards = allCards.Except(winningCards).ToList();
            DebugLogCards("CompleteWinningHand() remainingCards ", remainingCards);

            if (neededNumberOfCards < 1)
            {
                throw new Exception("Algo.CompleteWinningHand(): neededNumberOfCards is less than 1. Something is very wrong.");
            }

            while(neededNumberOfCards > 0)
            {
                completeHand.Insert(0, remainingCards.ElementAt(remainingCards.Count - 1));
                remainingCards.RemoveAt(remainingCards.Count - 1);
                neededNumberOfCards--;
            }

            if(completeHand.Count != 5){
                throw new Exception("Algo.CompleteWinningHand(): completeHand.Count != 5. Logic is wrong");
            }
            return completeHand;
        }

        private static List<Card> GetBestFiveCards(List<Card> cards)
        {
            if(cards.Count >= 5)
            {
                return cards.GetRange(cards.Count - 5, 5);
            }
            else
            {
                throw new Exception("⛔ Algo.GetBestFiveCards(): The List<Card> passed has less than 5 cards.");
            }
        }

        private static void SetWinningHand(Player player, HandType handType, List<Card> cards)
        {
            player.WinningHand = new WinningHand(handType, cards);
            if (_debugEnable)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($" {handType}: ");
                Console.ResetColor();
                foreach (Card c in cards)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = c.Suit == CardSuit.Spades || c.Suit == CardSuit.Clubs ? ConsoleColor.Black : ConsoleColor.Red;
                    Console.Write($"{c} ");
                    Console.ResetColor();
                }
                Console.WriteLine();
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


        private static void Testing_AddNoWinningHand(Player player){
            if (_unitTestingEnable)
            {
                player.WinningHand = new(HandType.Nothing, new List<Card>());
            }
        }

        private static void DebugLogCards(string description, List<Card> cards){
            if (_debugEnable)
            {
                Console.WriteLine($"🤖 {description}: ");
                Console.Write("\t" + string.Join(' ', cards) + "\n\n");
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
