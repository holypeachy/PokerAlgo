namespace PokerAlgo
{
    static partial class Algo
    {
        public static int _debugVerbosity = 0; // * Levels | 0 = Debug Logging disabled | 1 = Progress Report | 2 = Everything
        public static bool _unitTestingEnable = false; // TODO Work in Progress

        private static Dictionary<HandType, int> _numberOfCardsOfHand = new()
        {
            {HandType.Nothing, 0},
            {HandType.Pair, 2},
            {HandType.TwoPairs, 4},
            {HandType.ThreeKind, 3},
            {HandType.Straight, 5},
            {HandType.Flush, 5},
            {HandType.FullHouse, 5},
            {HandType.FourKind, 4},
            {HandType.StraightFlush, 5},
            {HandType.RoyalFlush, 5}
        };

        // ! This should be the only public method in this class.
        public static List<Player> GetWinners(List<Player> players, List<Card> communityCards)
        {
            if(players.Count < 2)
            {
                throw new Exception("⛔ Algo.FindWinner() - players.Count < 2. There must be at least 2 players.");
            }

            DebugLog("\n--- 🔎 Algo Starts");

            foreach (Player player in players)
            {
                if (_debugVerbosity > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("💭 Determining Hand for \'" + player.Name + "\'");
                    Console.ResetColor();
                }
                DeterminePlayerWinningHand(player, communityCards);
            }

            // Parallel.ForEach(
            //     players, player => {
            //         DeterminePlayerWinningHand(player, communityCards);
            //     }
            // );

            Player community = new("Community", new Card(0, CardSuit.Spades, true), new Card(0, CardSuit.Spades, true));

            if (_debugVerbosity > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n🃏 Determining Hand for Community");
                Console.ResetColor();
            }

            DetermineCommunityWinningHand(communityCards, community);

            DebugLog("\n--- 💭 Find Winners");

            List<Player> winners = DetermineWinners(players, community);

            return winners;

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

            EvaluateFlush(cards, player);
            DebugLog("", 2);
            EvaluateStraight(cards, player);
            DebugLog("", 2);
            EvaluateMultiples(cards, player);
        }

        // * Beautiful
        public static void EvaluateFlush(List<Card> combinedCards, Player player)
        {
            List<Card> flushCards = combinedCards.GroupBy(card => card.Suit)
            .Where(group => group.Count() >= 5)
            .SelectMany(group => group).ToList();

            if (flushCards.Count == 0)
            {
                DebugLog("⚠️  Algo.EvaluateFlush() - No Flush", 2);
                return;
            }

            DebugLogCards("Algo.EvaluateFlush() - Flush Cards", flushCards);

            List<Card> bestFive = new();

            // ! Royal Flush
            bestFive = GetBestFiveCards(flushCards);
            if(bestFive.ElementAt(0).Value == 10 && HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive)){
                SetWinningHand(player, HandType.RoyalFlush, bestFive);
                return;
            }

            AddLowAces(flushCards);

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

            flushCards = RemoveLowAces(flushCards);

            // ! Standard Flush
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

            DebugLog("⚠️  Algo.EvaluateFlush() - No Player Flush", 2);
        }

        // * Beautiful
        public static void EvaluateStraight(List<Card> cards, Player player){
            // ! If Player already has a winning hand, no need to execute this method
            if (player.WinningHand != null && player.WinningHand.Type > HandType.Straight)
            {
                DebugLog($"⚠️  Algo.EvaluateStraight() - Early return. Player already has a higher winning hand {player.WinningHand.Type.ToString()}.", 2);
                return;
            }

            // Shallow Copy, no need to deep copy here
            List<Card> tempCards = cards.ToList();
            AddLowAces(tempCards);

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

            DebugLogCards("Algo.EvaluateStraight() - Without Duplicates", tempCards);


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
            DebugLog("⚠️  Algo.EvaluateStraight() - No Straight", 2);
        }

        // * Okay for now, need to look at it again
        public static void EvaluateMultiples(List<Card> cards, Player player){
            // ! If Player already has a winning hand, no need to execute this method
            if (player.WinningHand != null && player.WinningHand.Type > HandType.FourKind)
            {
                DebugLog($"⚠️  Algo.EvaluateMultiples() - Early return at start. Player already has winning hand: {player.WinningHand.Type.ToString()}.", 2);
                return;
            }

            List<Card> duplicateCards = RemoveLowAces(cards); // ? Just in case

            duplicateCards = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group).ToList();

            if(duplicateCards.Count == 0){
                DebugLog("⚠️  Algo.EvaluateMultiples() - No Multiple", 2);
                if (player.WinningHand == null) SetWinningHand(player, HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));
                return;
            }

            DebugLogCards("Algo.EvaluateMultiples() - Duplicate Cards", duplicateCards);

            // ! Four of a Kind
            List<Card> fourKind = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 4).SelectMany(group => group).ToList();

            if(fourKind.Count == 4 && HasPlayerCard(duplicateCards)){
                SetWinningHand(player, HandType.FourKind, CompleteWinningHand(fourKind, cards));
                return;
            }
            else{
                DebugLog("⚠️  Algo.EvaluateMultiples() - No FourKind", 2);
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
                    throw new Exception("⛔ MultipleFinder() 2x, 3 of a kind. It's not possible for 2, 3Ks to exist and the player not have a card in.");
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
                    throw new Exception("⛔ MultipleFinder() 1x, 3 of a kind, 2 pairs. It's not possible for 1x 3K and 2 pairs to exist and the player not have a card in.");
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
            }

            if (player.WinningHand != null && player.WinningHand.Type > HandType.ThreeKind)
            {
                DebugLog($"\n⚠️  Algo.EvaluateMultiples() - Early return after Full House. Player already has winning hand: {player.WinningHand.Type.ToString()}.", 2);
                return;
            }

            // ! Three of a kind
            if (threeKinds.Count == 3)
            {
                if (HasPlayerCard(threeKinds))
                {
                    SetWinningHand(player, HandType.ThreeKind, CompleteWinningHand(threeKinds, cards));
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
                    throw new Exception("⛔ MultipleFinder() 3 pairs. It's not possible for 3 pairs to exist and the player not have a card in.");
                }
                twoPairs.AddRange(topPair);

                SetWinningHand(player, HandType.TwoPairs, CompleteWinningHand(twoPairs, cards));
            }
            
            // ! 2 Pairs
            else if(pairs.Count == 4){
                if (HasPlayerCard(pairs)){
                    SetWinningHand(player, HandType.TwoPairs, CompleteWinningHand(pairs, cards));
                }
            }
            
            // ! 1 Pair
            else if(pairs.Count == 2){
                if(HasPlayerCard(pairs)){
                    SetWinningHand(player, HandType.Pair, CompleteWinningHand(pairs,cards));
                }
            }

            // ! Nothing
            else{
                DebugLog("⚠️  Algo.EvaluateMultiples() - No Multiple", 2);
                if (player.WinningHand == null) SetWinningHand(player, HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));
            }
            
            if (player.WinningHand == null) SetWinningHand(player, HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));
        }


        // * Okay for now, need to optimize in the future
        public static void DetermineCommunityWinningHand(List<Card> communityCards, Player community)
        {
            // Shallow copy for ease
            List<Card> cards = communityCards.ToList();
            SortCardsByValue(cards);

            DebugLogCards("Algo.DetermineCommunityWinningHand() - Sorted Community Cards", cards);

            List<Card> bestFive = new();

            if (!IsSameSuit(cards))
            {
                DebugLog("⚠️  Algo.DetermineCommunityWinningHand() - No Flush\n", 2);
            }
            else{
                // ! Royal Flush
                bestFive = GetBestFiveCards(cards);
                if (bestFive.ElementAt(0).Value == 10 && HasConsecutiveValues(bestFive))
                {
                    SetWinningHand(community, HandType.RoyalFlush, bestFive);
                    return;
                }

                // ! Straight Flush
                AddLowAces(cards);
                // cards.AddLowAces();
                for (int i = cards.Count - 5; i >= 0; i--)
                {
                    bestFive = cards.GetRange(i, 5);
                    if (HasConsecutiveValues(bestFive))
                    {
                        SetWinningHand(community, HandType.StraightFlush, bestFive);
                        return;
                    }
                }
                cards = RemoveLowAces(cards);

                // ! Standard Flush
                SetWinningHand(community, HandType.Flush, cards);
                return;
            }

            List<Card> tempCards = cards.ToList();

            // ! Removes duplicates
            for (int i = tempCards.Count - 1; i > 0; i--)
            {
                if (tempCards[i].Value == tempCards[i - 1].Value)
                {
                    if (tempCards[i].IsPlayerCard && tempCards[i - 1].IsPlayerCard || !tempCards[i].IsPlayerCard && !tempCards[i - 1].IsPlayerCard)
                    {
                        tempCards.RemoveAt(i);
                    }
                    else if (tempCards[i].IsPlayerCard)
                    {
                        tempCards.RemoveAt(i - 1);
                    }
                    else
                    {
                        tempCards.RemoveAt(i);
                    }
                }
            }

            DebugLogCards("Algo.DetermineCommunityWinningHand() - Without Duplicates", tempCards);
            // ! Straight
            AddLowAces(tempCards);
            if(tempCards.Count >= 5){
                for (int i = tempCards.Count - 5; i >= 0; i--)
                {
                    bestFive = tempCards.GetRange(i, 5);
                    if (HasConsecutiveValues(bestFive) && !IsSameSuit(bestFive))
                    {
                        SetWinningHand(community, HandType.Straight, bestFive);
                        return;
                    }
                }
            }
            tempCards = RemoveLowAces(tempCards);

            DebugLog("⚠️  Algo.DetermineCommunityWinningHand() - No Straight\n", 2);

            List<Card> duplicateCards = cards.GroupBy(card => card.Value)
                            .Where(group => group.Count() > 1)
                            .SelectMany(group => group).ToList();

            if (duplicateCards.Count == 0)
            {
                DebugLog("⚠️  Algo.DetermineCommunityWinningHand() - No Multiple\n", 2);
                SetWinningHand(community, HandType.Nothing, cards);
                return;
            }

            // ! Four of a Kind
            List<Card> fourKind = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() == 4).SelectMany(group => group).ToList();

            if (fourKind.Count == 4)
            {
                SetWinningHand(community, HandType.FourKind, CompleteWinningHand(fourKind, cards));
                return;
            }

            DebugLog("⚠️  Algo.DetermineCommunityWinningHand() - No FourKind", 2);


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

            // ! Three of a kind
            else if (threeKinds.Count == 3)
            {
                SetWinningHand(community, HandType.ThreeKind, CompleteWinningHand(threeKinds, cards));
            }

            // ! 2 Pairs
            else if (pairs.Count == 4)
            {
                SetWinningHand(community, HandType.TwoPairs, CompleteWinningHand(pairs, cards));
            }

            // ! 1 Pair
            else if (pairs.Count == 2)
            {
                SetWinningHand(community, HandType.Pair, CompleteWinningHand(pairs, cards));
            }
            else
            {
                SetWinningHand(community, HandType.Nothing, cards);
            }

            return;
        }


        public static List<Player> DetermineWinners(List<Player> allPlayers, Player community)
        {
            // ! Temporary, shouldn't need this after proper testing. ???
            if (community.WinningHand is null)
            {
                throw new Exception("⛔ Algo.FindWinners() - Community WinningHand is null.");
            }
            foreach (Player p in allPlayers)
            {
                if (p.WinningHand is null)
                {
                    throw new Exception($"⛔ Algo.FindWinners() - Player\'s \'{p.Name}\' WinningHand is null.");
                }
            }

            // ! Order from highest to lowest hand value
            List<Player> players = allPlayers.OrderByDescending(x => x.WinningHand.Type).ToList();

            DebugLogPlayers("Algo.FindWinners() - Players after sorting by WinningHand.Type", players, community);

            DebugLog("", 2);

            // ! If community is greater. 2 options:
            if(community.WinningHand.Type > players.ElementAt(0).WinningHand.Type)
            {
                // ! If community has best 5 cards then tie among all players
                if(_numberOfCardsOfHand[community.WinningHand.Type] == 5)
                {
                    DebugLog("Algo.DetermineWinners() - Community Hand is Better | 5 Card Hand | Tie Among All Players\n", 1);

                    return players;
                }

                // ! If community has less than 5 best cards then tie break players.
                else
                {
                    DebugLog($"Algo.DetermineWinners() - Community Hand is Better | {_numberOfCardsOfHand[community.WinningHand.Type]} Card Hand | Tie Break Players\n", 1);
                    List<Player> winners = BreakTieCommunityLessThanFiveCards(players, community);

                    return winners;
                }
            }

            // ! If community and player are the same. Tie break everything. If community wins it's a tie
            else if (community.WinningHand.Type == players.ElementAt(0).WinningHand.Type)
            {
                DebugLog("Algo.DetermineWinners() - Community Hand is Equal | Tie Break Community + Players\n", 1);
                players.Add(community);
                List<Player> winners = BreakTies(players);

                if(winners.Count == 1 && winners.Contains(community))
                {
                    DebugLog("Algo.DetermineWinners() - Community Wins | Everyone Ties\n", 1);
                }
                else
                {
                    winners.Remove(community);

                }
                return winners;
            }
            
            // ! If the community hand is worse then we proceed as normal
            else
            {
                DebugLog("Algo.DetermineWinners() - Players Best Hand is Better | Tie Break Players Only\n", 1);
                List<Player> winners = BreakTies(players);

                return winners;
            }
            
        }


        private static List<Player> BreakTies(List<Player> players)
        {
            List<Player> winners = players.ToList();

            bool hasChangesBeenMade;

            List<Player> tempPlayers = winners.ToList();
            do
            {
                hasChangesBeenMade = false;
                for (int playerIndex = 0; playerIndex < winners.Count - 1; playerIndex++)
                {
                    int result = ComparePlayerHands(winners[playerIndex], winners[playerIndex + 1]);
                    DebugLog($"Algo.BreakTies() - " + (result == -1 ? winners[playerIndex].Name + " has better hand\n" : result == 1 ? winners[playerIndex + 1].Name + " has better hand\n" : "Players Tie"), 2);

                    if (result == -1)
                    {
                        tempPlayers.Remove(winners.ElementAt(playerIndex + 1));
                        hasChangesBeenMade = true;
                    }
                    else if (result == 1)
                    {
                        tempPlayers.Remove(winners.ElementAt(playerIndex));
                        hasChangesBeenMade = true;
                    }
                    else if (result == 0){

                    }
                    else{
                        throw new Exception("⛔ Algo.BreakTies() - ComparePlayerHands() returned something other than -1, 0, or 1.");
                    }
                }
                winners = tempPlayers;
            } while (hasChangesBeenMade && winners.Count > 1);

            return winners;
        }

        private static List<Player> BreakTieCommunityLessThanFiveCards(List<Player> allPlayers, Player community)
        {
            int numberOfHand = _numberOfCardsOfHand[community.WinningHand.Type];
            int numberRemaining = 5 - _numberOfCardsOfHand[community.WinningHand.Type];

            List<Player> winners = allPlayers.ToList();

            List<Card> winningCards = community.WinningHand.Cards.GetRange(5 - numberOfHand, numberOfHand);

            List<Player> tempPlayers = allPlayers.ToList();

            List<Card> tempLeft;
            List<Card> tempRight;
            bool hasChangesBeenMade;
            do
            {
                hasChangesBeenMade = false;
                for (int i = 0; i < winners.Count - 1; i++)
                {
                    DebugLog($"Algo.BreakTieCommunityLessThanFiveCards() - {winners.ElementAt(i).Name} | Hand: {winners.ElementAt(i).WinningHand.Type}", 2);
                    DebugLog($"Algo.BreakTieCommunityLessThanFiveCards() - {winners.ElementAt(i+1).Name} | Hand: {winners.ElementAt(i + 1).WinningHand.Type}", 2);

                    tempLeft = winners.ElementAt(i).WinningHand.Cards.ToList().Except(winningCards).ToList();
                    tempLeft = tempLeft.GetRange(tempLeft.Count - numberRemaining, numberRemaining);

                    tempRight = winners.ElementAt(i+1).WinningHand.Cards.ToList().Except(winningCards).ToList();
                    tempRight = tempRight.GetRange(tempRight.Count - numberRemaining, numberRemaining);

                    int result = CompareKickers(tempLeft, tempRight);
                    DebugLog($"Algo.BreakTieCommunityLessThanFiveCards() - " + (result == -1 ? winners[i].Name + " has better hand\n" : result == 1 ? winners[i + 1].Name + " has better hand\n" : "Players Tie"), 2);

                    if (result == -1)
                    {
                        tempPlayers.Remove(winners.ElementAt(i + 1));
                        hasChangesBeenMade = true;
                    }
                    else if (result == 1)
                    {
                        tempPlayers.Remove(winners.ElementAt(i));
                        hasChangesBeenMade = true;
                    }
                    else if (result == 0)
                    {

                    }
                    else
                    {
                        throw new Exception("⛔ Algo.Community_BreakTieLessFive2(): CompareKickers() returned something other than -1, 0, or 1.");
                    }
                }
                winners = tempPlayers;
            } while (hasChangesBeenMade && winners.Count > 1);

            return winners;
        }

    }
}
