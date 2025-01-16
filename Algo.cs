namespace PokerAlgo
{
    static class Algo
    {
        public static int _debugVerbosity = 1; // * Levels | 0 = Debug Logging disabled | 1 = Progress Report | 2 = Everything
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
        public static void DetermineWinner(List<Player> players, List<Card> communityCards)
        {
            if(players.Count < 2)
            {
                throw new Exception("Algo.FindWinner() - players.Count < 2. There must be at least 2 players.");
            }

            if(_debugVerbosity > 0) Console.WriteLine("\n--- 🔎 Algo Starts");

            foreach (Player player in players)
            {
                if (_debugVerbosity > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("💭 Determining Hand for \'" + player.Name + "\'\n");
                    Console.ResetColor();
                }
                DeterminePlayerWinningHand(player, communityCards);
            }

            Player community = new("Community", new Card(0, CardSuit.Spades, true), new Card(0, CardSuit.Spades, true));

            if (_debugVerbosity > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n🃏 Determining Hand for Community\n");
                Console.ResetColor();
            }

            DetermineCommunityWinningHand(communityCards, community);

            if (_debugVerbosity > 0)
            {
                Console.WriteLine("\n--- 💭 Find Winners");
            }

            FindWinners(players, community);

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


            FindFlush(cards, player);
            if (_debugVerbosity > 1) Console.WriteLine();
            // cards.AddLowAces();
            AddLowAces(cards);
            FindStraight(cards, player);
            if (_debugVerbosity > 1) Console.WriteLine();
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
                if(_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.FindFlush() - No Flush");
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

            if (_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.FindFlush() - No Player Flush");
            Testing_AddNoWinningHand(player);
        }

        // * Beautiful
        public static void FindStraight(List<Card> cards, Player player){
            // ! If Player already has a winning hand, no need to execute this method
            if (player.WinningHand != null && player.WinningHand.Type > HandType.Straight)
            {
                if (_debugVerbosity > 1) Console.WriteLine($"⚠️  Algo.FindStraight() - Early return. Player already has a higher winning hand {player.WinningHand.Type.ToString()}.");
                return;
            }

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
            if (_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.FindStraight() - No Straight");
            Testing_AddNoWinningHand(player);
        }

        // * Okay for now, need to look at it again
        public static void FindMultiple(List<Card> cards, Player player){
            // ! If Player already has a winning hand, no need to execute this method
            if (player.WinningHand != null && player.WinningHand.Type > HandType.FourKind)
            {
                if (_debugVerbosity > 1) Console.WriteLine($"⚠️  Algo.FindMultiple() - Early return at start. Player already has winning hand: {player.WinningHand.Type.ToString()}.");
                return;
            }

            List<Card> duplicateCards = RemoveLowAces(cards);

            duplicateCards = duplicateCards.GroupBy(card => card.Value)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group).ToList();

            if(duplicateCards.Count == 0){
                if (_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.FindMultiple() - No Multiple");
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
                if (_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.FindMultiple() - No FourKind");
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

            if (player.WinningHand != null && player.WinningHand.Type > HandType.ThreeKind)
            {
                if (_debugVerbosity > 1) Console.WriteLine($"\n⚠️  Algo.FindMultiple() - Early return after Full House. Player already has winning hand: {player.WinningHand.Type.ToString()}.");
                return;
            }

            // ! Three of a kind
            if (threeKinds.Count == 3)
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
                if (_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.FindMultiple() - No Multiple");
                if (player.WinningHand == null) SetWinningHand(player, HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));

                Testing_AddNoWinningHand(player);
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
                if (_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.DetermineCommunityWinningHand() - No Flush\n");
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
                RemoveLowAces(cards);

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

            if (_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.DetermineCommunityWinningHand() - No Straight\n");

            List<Card> duplicateCards = cards.GroupBy(card => card.Value)
                            .Where(group => group.Count() > 1)
                            .SelectMany(group => group).ToList();

            if (duplicateCards.Count == 0)
            {
                if (_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.DetermineCommunityWinningHand() - No Multiple\n");
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

            if (_debugVerbosity > 1) Console.WriteLine("⚠️  Algo.DetermineCommunityWinningHand() - No FourKind");


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


        public static void FindWinners(List<Player> allPlayers, Player community)
        {
            // ! Temporary, shouldn't need this after proper testing. ???
            if (community.WinningHand is null)
            {
                throw new Exception("Algo.FindWinners() - Community WinningHand is null. This shouldn't happen");
            }
            foreach (Player p in allPlayers)
            {
                if (p.WinningHand is null)
                {
                    throw new Exception($"Algo.FindWinners() - Player\'s \'{p.Name}\' WinningHand is null. This shouldn't happen.");
                }
            }

            // ! Order from highest to lowest hand value
            List<Player> players = allPlayers.OrderByDescending(x => x.WinningHand.Type).ToList();

            DebugLogPlayers("Algo.FindWinners() - Players after sorting by WinningHand.Type", players, community);

            if (_debugVerbosity > 0) Console.WriteLine();

            // ! If community is greater. 2 options:
            if(community.WinningHand.Type > players.ElementAt(0).WinningHand.Type)
            {
                // ! If community has best 5 cards then tie among all players
                if(_numberOfCardsOfHand[community.WinningHand.Type] == 5)
                {
                    Console.WriteLine("TIE AMONG ALL PLAYERS");
                    foreach (Player p in players)
                    {
                        p.IsWinner = true;
                    }
                }

                // ! If community has less than 5 best cards then tie break players.
                else
                {
                    Console.WriteLine($"COMMUNITY HAND GREATER: WE NEED TO TIE BREAK PLAYERS BECAUSE NUMBER OF CARDS: {_numberOfCardsOfHand[community.WinningHand.Type]}");
                    Community_BreakTieLessFive(players, community);
                }
            }

            // ! If community and player are the same. Tie break everything. If community wins it's a tie
            else if (community.WinningHand.Type == players.ElementAt(0).WinningHand.Type)
            {
                Console.WriteLine("COMMUNITY == PLAYER. NEED TO BREAK TIE. IF COMMUNITY WINS IT'S TIE");
                players.Add(community);
                List<Player> winners = BreakTies(players);

                if(winners.Count == 1 && winners.Contains(community))
                {
                    Console.WriteLine("EVERYONE TIES");
                }
                else
                {
                    winners.Remove(community);
                    Console.WriteLine(); 
                    Console.WriteLine("WINNERS:");
                    foreach (var p in winners)
                    {
                        Console.WriteLine(p);
                    }
                }
            }
            
            // ! If the community hand is worse then we proceed as usual
            else if (community.WinningHand.Type < players.ElementAt(0).WinningHand.Type)
            {
                Console.WriteLine("NEED TO BREAK TIE AMONG PLAYERS **ONLY**");
                List<Player> winners = BreakTies(players);
                Console.WriteLine("\nWINNERS: ");
                foreach (var p in winners)
                {
                    Console.WriteLine(p.Name + string.Join(' ', p.WinningHand.Cards));
                }
            }
            
            else
            {
                throw new Exception("what the actual fuck");
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
                    else if(result == 0){
                        Console.WriteLine("Same");
                    }
                    else{
                        throw new Exception("Something broke");
                    }
                }
                winners = tempPlayers;
            } while (hasChangesBeenMade);

            return winners;
        }


        // TODO: Take a look at these again
        private static void Community_BreakTieLessFive(List<Player> allPlayers, Player community)
        {
            int numberOfHand = _numberOfCardsOfHand[community.WinningHand.Type];
            int numberRemaining = 5 - _numberOfCardsOfHand[community.WinningHand.Type];

            List<Card> communityCards = community.WinningHand.Cards.GetRange(5 - numberOfHand, numberOfHand);

            List<PlayerWinningObj> winningObjs = new();
            foreach (Player p in allPlayers)
            {
                winningObjs.Add(new PlayerWinningObj(p, p.WinningHand.Cards.Except(communityCards).ToList()));
            }

            PlayerWinningObj current;
            PlayerWinningObj next;
            List<PlayerWinningObj> tempWinningObjs = winningObjs.ToList();
            bool hasChangesBeenMade;
            do
            {
                hasChangesBeenMade = false;
                for (int playerIndex = 0; playerIndex < winningObjs.Count - 1; playerIndex++)
                {
                    current = winningObjs[playerIndex];
                    next = winningObjs[playerIndex + 1];

                    for (int cardsIndex = 0; cardsIndex < numberRemaining; cardsIndex++)
                    {
                        if(current.Cards[current.Cards.Count - 1 - cardsIndex].Value < next.Cards[next.Cards.Count - 1 - cardsIndex].Value)
                        {
                            tempWinningObjs.Remove(current);
                            hasChangesBeenMade = true;
                        }
                        else if (current.Cards[current.Cards.Count - 1 - cardsIndex].Value > next.Cards[next.Cards.Count - 1 - cardsIndex].Value)
                        {
                            tempWinningObjs.Remove(next);
                            hasChangesBeenMade = true;
                        }
                    }
                }
                winningObjs = tempWinningObjs.ToList();
            }while(hasChangesBeenMade);

            Console.WriteLine("Winners and their Tie Breakers:");
            foreach (PlayerWinningObj o in winningObjs)
            {
                Console.Write(o.Owner.Name + ":\n\t");
                foreach (var c in o.Cards.GetRange(o.Cards.Count - numberRemaining, numberRemaining))
                {
                    Console.Write(c + " ");
                }
                Console.WriteLine();
            }
        }

        // ! -1 left wins, 0 tie, 1 right wins
        private static int ComparePlayerHands(Player player1, Player player2)
        {
            if (player1.WinningHand is null || player2.WinningHand is null) throw new Exception("Algo.ComparePlayerHands(): A player's winning hand was null, something went very wrong.");

            WinningHand left = player1.WinningHand;
            WinningHand right = player2.WinningHand;

            // * left.Type = HandType.RoyalFlush;
            // * right.Type = HandType.RoyalFlush;

            Console.WriteLine($"Algo.ComparePlayerHands() - {player1.Name}");
            Console.WriteLine($"Algo.ComparePlayerHands() - {player2.Name}");

            if (left.Type > right.Type)
            {
                Console.WriteLine($"Algo.ComparePlayerHands() - {player1.Name} is better");
                return -1;
            }
            else if(right.Type > left.Type)
            {
                Console.WriteLine($"Algo.ComparePlayerHands() - {player2.Name} is better");
                return 1;
            }

            List<Card> leftCards = left.Cards;
            List<Card> rightCards = right.Cards;

            Console.WriteLine("Algo.ComparePlayerHands() - Type: " + left.Type);
            switch (left.Type)
            {
                case HandType.RoyalFlush:
                    Console.WriteLine("Algo.ComparePlayerHands(): Since only one player can have a royal flush it makes no sense that it would get to this point. "
                                        + "Also since my code does not give community winning hands to the players, no two players can have a royal flush.");
                    return 0;

                case HandType.StraightFlush:
                    return CompareKickers(leftCards, rightCards);

                case HandType.FourKind:
                    if (leftCards.ElementAt(4).Value > rightCards.ElementAt(4).Value) return -1;
                    else if (rightCards.ElementAt(4).Value > leftCards.ElementAt(4).Value) return 1;
                    else return CompareKickers(new List<Card> { leftCards.ElementAt(0) }, new List<Card> { rightCards.ElementAt(0) });

                case HandType.FullHouse:
                    if (leftCards.ElementAt(0).Value > rightCards.ElementAt(0).Value) return -1;
                    else if (rightCards.ElementAt(0).Value > leftCards.ElementAt(0).Value) return 1;
                    else if (leftCards.ElementAt(3).Value > rightCards.ElementAt(3).Value) return -1;
                    else if (rightCards.ElementAt(3).Value > leftCards.ElementAt(3).Value) return 1;
                    else return 0;

                case HandType.Flush:
                    return CompareKickers(leftCards, rightCards);

                case HandType.Straight:
                    return CompareKickers(leftCards, rightCards);

                case HandType.ThreeKind:
                    if (leftCards.ElementAt(4).Value > rightCards.ElementAt(4).Value) return -1;
                    else if (rightCards.ElementAt(4).Value > leftCards.ElementAt(4).Value) return 1;
                    else return CompareKickers(leftCards.GetRange(0, 2), rightCards.GetRange(0, 2));

                case HandType.TwoPairs:
                    if (leftCards.ElementAt(4).Value > rightCards.ElementAt(4).Value) return -1;
                    else if (rightCards.ElementAt(4).Value > leftCards.ElementAt(4).Value) return 1;
                    if (leftCards.ElementAt(2).Value > rightCards.ElementAt(2).Value) return -1;
                    else if (rightCards.ElementAt(2).Value > leftCards.ElementAt(2).Value) return 1;

                    else return CompareKickers(new List<Card> { leftCards.ElementAt(0) }, new List<Card> { rightCards.ElementAt(0) });


                case HandType.Pair:
                    if (leftCards.ElementAt(4).Value > rightCards.ElementAt(4).Value) return -1;
                    else if (rightCards.ElementAt(4).Value > leftCards.ElementAt(4).Value) return 1;
                    else return CompareKickers(leftCards.GetRange(0, 3), rightCards.GetRange(0, 3));

                case HandType.Nothing:
                    return CompareKickers(leftCards, rightCards);

                default:
                    throw new Exception("Algo.ComparePlayerHands(): Switch defaulted.");
            }

            throw new NotImplementedException();
        }

        // ! -1 left wins, 0 tie, 1 right wins
        private static int CompareKickers(List<Card> left, List<Card> right)
        {
            if (left.Count != right.Count) throw new Exception("⛔ Algo.CompareKickers(): left.Count != right.Count.");

            DebugLogCards("Algo.CompareKickers() - Left", left);
            DebugLogCards("Algo.CompareKickers() - Right", right);

            for (int i = left.Count - 1; i >= 0; i--)
            {
                if(left.ElementAt(i).Value > right.ElementAt(i).Value)
                {
                    return -1;
                }
                else if (right.ElementAt(i).Value > left.ElementAt(i).Value)
                {
                    return 1;
                }
            }

            return 0;
        }


        // * Helper Methods
        private static List<Card> CompleteWinningHand(List<Card> winningCards, List<Card> allCards)
        {
            List<Card> completeHand = winningCards.ToList();
            int neededNumberOfCards = 5 - winningCards.Count;
            List<Card> remainingCards = allCards.Except(winningCards).ToList();
            if(_debugVerbosity > 1) Console.WriteLine();
            DebugLogCards("Algo.CompleteWinningHand() - remainingCards", remainingCards);

            if (neededNumberOfCards < 1)
            {
                throw new Exception("⛔ Algo.CompleteWinningHand(): neededNumberOfCards is less than 1. Something is very wrong.");
            }

            while(neededNumberOfCards > 0)
            {
                completeHand.Insert(0, remainingCards.ElementAt(remainingCards.Count - 1));
                remainingCards.RemoveAt(remainingCards.Count - 1);
                neededNumberOfCards--;
            }

            if(completeHand.Count != 5){
                throw new Exception("⛔ Algo.CompleteWinningHand(): completeHand.Count != 5. Logic is wrong.");
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

            if (_debugVerbosity > 0)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($" {handType}: ");
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write(" ");
                foreach (Card c in cards)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
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


        // ! Testing_AddNoWinningHand is Depricated
        private static void Testing_AddNoWinningHand(Player player){
            if (_unitTestingEnable)
            {
                player.WinningHand = new(HandType.Nothing, new List<Card>());
            }
        }


        private static void DebugLogCards(string description, List<Card> cards){
            if (_debugVerbosity > 1)
            {
                Console.WriteLine($"🤖 {description}: ");
                Console.Write("\t" + string.Join(' ', cards) + "\n\n");
            }
        }

        private static void DebugLogPlayers(string description, List<Player> players)
        {
            if (_debugVerbosity > 0)
            {
                foreach (Player p in players)
                {
                    Console.Write($"\t 🃏 Winning Hand ");
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Write($" {p.Name} ");
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write($" {p.WinningHand.Type} ");
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(string.Join(' ', p.WinningHand.Cards) + " ");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

        private static void DebugLogPlayers(string description, List<Player> players, Player community)
        {
            if (_debugVerbosity > 0)
            {
                Console.WriteLine($"🤖 {description}:");
                Console.Write($"\t 🃏 Winning Hand ");
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write($" {community.Name} ");
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write($" {community.WinningHand.Type} ");
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write(string.Join(' ', community.WinningHand.Cards) + " ");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine();
                foreach (Player p in players)
                {
                    Console.Write($"\t 🃏 Winning Hand ");
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Write($" {p.Name} ");
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write($" {p.WinningHand.Type} ");
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(string.Join(' ', p.WinningHand.Cards) + " ");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine();
                }
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
                    if (cards[i].Value != ++startingValue)
                    {
                        return false;
                    }
                    // startingValue++;
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
