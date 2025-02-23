namespace PokerAlgo
{
    public static class Algo
    {
        private static int _debugVerbosity = 0;
        public static int DebugVerbosity {
            get
            { 
                return _debugVerbosity;
            }
            set
            { 
                if(value == 0 || value == 1 || value == 2)
                {
                    _debugVerbosity = value;
                }
                else
                {
                    _debugVerbosity = 0;
                }
            }
        } // * Verbosity Levels | 0 = Disabled | 1 = Progress Report | 2 = Everything

        public static List<Player> GetWinners(List<Player> players, List<Card> communityCards)
        {
            if(players.Count < 2)
            {
                throw new Exception("⛔ Algo.FindWinner() - players.Count < 2. There must be at least 2 players.");
            }

            Helpers.DebugLog("\n--- 🔎 Algo Starts");
            HandEvaluator handEvaluator = new HandEvaluator();
            List<Card> combinedCards;

            foreach (Player player in players)
            {
                Helpers.DebugLogDeterminingHand(player.Name);

                combinedCards = new()
                {
                    player.HoleCards.First,
                    player.HoleCards.Second
                };
                combinedCards.AddRange(communityCards);

                player.WinningHand = handEvaluator.GetWinningHand(combinedCards);
            }

            // community.WinningHand = handEvaluator.GetCommunityWinningHand(communityCards);

            Helpers.DebugLog("\n--- 💭 Find Winners");

            List<Player> winners = DetermineWinners(players);

            // Helpers.DebugLogWinners(winners);

            return winners;
        }

        private static List<Player> DetermineWinners(List<Player> allPlayers)
        {
            // ! Temporary, shouldn't need this after proper testing. ???
            foreach (Player p in allPlayers)
            {
                if (p.WinningHand is null)
                {
                    throw new Exception($"⛔ Algo.DetermineWinners() - Player\'s \'{p.Name}\' WinningHand is null.");
                }
            }

            // ! Order from highest to lowest hand value
            List<Player> players = allPlayers.OrderByDescending(x => x.WinningHand.Type).ToList();

            Helpers.DebugLogPlayers("Algo.DetermineWinners() - Players after sorting by WinningHand.Type", players);

            List<Player> winners =  BreakTies(players);

            if (winners.Count < 1) throw new Exception("Algo.DetermineWinners() - winners.Count < 1. This should never happen.");

            return winners;
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
                    int result = CompareWinningHands(winners[playerIndex].WinningHand, winners[playerIndex + 1].WinningHand);
                    Helpers.DebugLog($"Algo.BreakTies() - " + (result == -1 ? winners[playerIndex].Name + " has better hand\n" : result == 1 ? winners[playerIndex + 1].Name + " has better hand\n" : "Players Tie"), 2);

                    if (result == -1)
                    {
                        if(winners[playerIndex].WinningHand.Type > winners[playerIndex + 1].WinningHand.Type)
                        {
                            for (int k = playerIndex + 1; k < winners.Count; k++)
                            {
                                tempPlayers.Remove(winners.ElementAt(k));
                            }
                        }
                        else
                        {
                            tempPlayers.Remove(winners.ElementAt(playerIndex + 1));
                        }
                        hasChangesBeenMade = true;
                        break;
                    }
                    else if (result == 1)
                    {
                        tempPlayers.Remove(winners.ElementAt(playerIndex));
                        hasChangesBeenMade = true;
                    }
                    else if (result == 0){}
                    else
                    {
                        throw new Exception("⛔ Algo.BreakTies() - ComparePlayerHands() returned something other than -1, 0, or 1.");
                    }
                }
                winners = tempPlayers;
            } while (hasChangesBeenMade && winners.Count > 1);

            return winners;
        }


        // ! -1 left wins, 0 tie, 1 right wins
        private static int CompareWinningHands(WinningHand? left, WinningHand? right)
        {
            if (left is null || right is null) throw new Exception("⛔ Algo.ComparePlayerHands(): A passed winning hand argument is null.");
            
            Helpers.DebugLogCards("Algo.CompareWinningHands() - Left.Cards", left.Cards);
            Helpers.DebugLogCards("Algo.CompareWinningHands() - Right.Cards", right.Cards);

            if (left.Type > right.Type)
            {
                return -1;
            }
            else if (right.Type > left.Type)
            {
                return 1;
            }

            List<Card> leftCards = left.Cards;
            List<Card> rightCards = right.Cards;

            switch (left.Type)
            {
                case HandType.RoyalFlush:
                    return 0;

                case HandType.StraightFlush:
                    return CompareKickers(leftCards, rightCards);

                case HandType.FourKind:
                    if (leftCards.ElementAt(4).Rank > rightCards.ElementAt(4).Rank) return -1;
                    else if (rightCards.ElementAt(4).Rank > leftCards.ElementAt(4).Rank) return 1;
                    else return CompareKickers(new List<Card> { leftCards.ElementAt(0) }, new List<Card> { rightCards.ElementAt(0) });

                case HandType.FullHouse:
                    if (leftCards.ElementAt(4).Rank > rightCards.ElementAt(4).Rank) return -1;
                    else if (rightCards.ElementAt(4).Rank > leftCards.ElementAt(4).Rank) return 1;
                    else if (leftCards.ElementAt(0).Rank > rightCards.ElementAt(0).Rank) return -1;
                    else if (rightCards.ElementAt(0).Rank > leftCards.ElementAt(0).Rank) return 1;
                    else return 0;

                case HandType.Flush:
                    return CompareKickers(leftCards, rightCards);

                case HandType.Straight:
                    return CompareKickers(leftCards, rightCards);

                case HandType.ThreeKind:
                    if (leftCards.ElementAt(4).Rank > rightCards.ElementAt(4).Rank) return -1;
                    else if (rightCards.ElementAt(4).Rank > leftCards.ElementAt(4).Rank) return 1;
                    else return CompareKickers(leftCards.GetRange(0, 2), rightCards.GetRange(0, 2));

                case HandType.TwoPair:
                    if (leftCards.ElementAt(4).Rank > rightCards.ElementAt(4).Rank) return -1;
                    else if (rightCards.ElementAt(4).Rank > leftCards.ElementAt(4).Rank) return 1;
                    if (leftCards.ElementAt(2).Rank > rightCards.ElementAt(2).Rank) return -1;
                    else if (rightCards.ElementAt(2).Rank > leftCards.ElementAt(2).Rank) return 1;

                    else return CompareKickers(new List<Card> { leftCards.ElementAt(0) }, new List<Card> { rightCards.ElementAt(0) });


                case HandType.Pair:
                    if (leftCards.ElementAt(4).Rank > rightCards.ElementAt(4).Rank) return -1;
                    else if (rightCards.ElementAt(4).Rank > leftCards.ElementAt(4).Rank) return 1;
                    else return CompareKickers(leftCards.GetRange(0, 3), rightCards.GetRange(0, 3));

                case HandType.Nothing:
                    return CompareKickers(leftCards, rightCards);

                default:
                    throw new Exception("⛔ Algo.ComparePlayerHands(): Switch defaulted. Was HandType enum changed?");
            }
        }

        // ! -1 left wins, 0 tie, 1 right wins
        private static int CompareKickers(List<Card> left, List<Card> right)
        {
            Helpers.DebugLogCards("Algo.CompareKickers() - Left", left);
            Helpers.DebugLogCards("Algo.CompareKickers() - Right", right);

            if (left.Count != right.Count)
            {
                throw new Exception("⛔ Algo.CompareKickers(): left.Count != right.Count." + "\nLeft: " + string.Join(' ', left) + "\nRight: " + string.Join(' ', right));
            }

            for (int i = left.Count - 1; i >= 0; i--)
            {
                if (left.ElementAt(i).Rank > right.ElementAt(i).Rank)
                {
                    Helpers.DebugLog("Algo.CompareKickers() - Left Wins", 2);
                    return -1;
                }
                else if (right.ElementAt(i).Rank > left.ElementAt(i).Rank)
                {
                    Helpers.DebugLog("Algo.CompareKickers() - Right Wins", 2);
                    return 1;
                }
            }
            Helpers.DebugLog("Algo.CompareKickers() - Tie", 2);

            return 0;
        }

    }
}
