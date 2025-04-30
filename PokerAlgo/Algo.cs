namespace PokerAlgo;
/// <summary>
/// Provides logic for evaluating and comparing players' hands in a Texas Hold'em game.
/// </summary>
public static class Algo
{
    /// <summary>
    /// Evaluates the hands of all players using the provided community cards and returns the winner(s).
    /// </summary>
    /// <param name="players">A list of players, each with hole cards.</param>
    /// <param name="communityCards">Exactly 5 community cards.</param>
    /// <returns>A list of winning players — one or more in the case of a tie.</returns>
    public static List<Player> GetWinners(List<Player> players, List<Card> communityCards)
    {
        Guards.ArgsGetWinners(players, communityCards);

        Helpers.DebugLog("--- 🔎 Algo Starts");
        HandEvaluator handEvaluator = new();
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

        Helpers.DebugLog("\n--- 💭 Find Winners");

        List<Player> winners = DetermineWinners(players);

        return winners;
    }

    private static List<Player> DetermineWinners(List<Player> allPlayers)
    {
        foreach (Player p in allPlayers)
        {
            if (p.WinningHand is null)
            {
                throw new InternalPokerAlgoException($"Invariant violated: a player's hand is null. Player\'s \'{p.Name}\' WinningHand is null. ");
            }
        }

        // Order from highest to lowest hand value
        List<Player> players = allPlayers.OrderByDescending(x => x.WinningHand.Type).ToList();

        Helpers.DebugLogPlayers("Algo.DetermineWinners() - Players after sorting by WinningHand.Type", players);

        List<Player> winners =  BreakTies(players);

        if (winners.Count < 1) throw new InternalPokerAlgoException("Invariant violated: winners.Count < 1. This should never happen.");

        return winners;
    }


    private static List<Player> BreakTies(List<Player> players)
    {
        List<Player> winners = players.ToList();
        List<Player> tempPlayers = winners.ToList();

        bool hasChangesBeenMade;
        do
        {
            hasChangesBeenMade = false;
            for (int playerIndex = 0; playerIndex < winners.Count - 1; playerIndex++)
            {
                int result = CompareWinningHands(winners[playerIndex].WinningHand, winners[playerIndex + 1].WinningHand);
                Helpers.DebugLog($"Algo.BreakTies() - " + (result == -1 ? winners[playerIndex].Name + " has the better hand\n" : result == 1 ? winners[playerIndex + 1].Name + " has the better hand\n" : $"Players Tie ({winners[playerIndex].Name} & {winners[playerIndex + 1].Name}"), 2);

                if (result == -1)
                {
                    if(winners[playerIndex].WinningHand.Type > winners[playerIndex + 1].WinningHand.Type)
                    {
                        Helpers.DebugLog("Algo.BreakTies() - Winning hand type difference, early break\n", 2);

                        for (int k = playerIndex + 1; k < winners.Count; k++)
                        {
                            tempPlayers.Remove(winners[k]);
                        }
                    }
                    else
                    {
                        tempPlayers.Remove(winners[playerIndex + 1]);
                    }
                    hasChangesBeenMade = true;
                    break;
                }
                else if (result == 1)
                {
                    tempPlayers.Remove(winners[playerIndex]);
                    hasChangesBeenMade = true;
                }
                else if (result == 0){}
                else
                {
                    throw new InternalPokerAlgoException("Invariant violated: CompareWinningHands() returned something other than -1, 0, or 1.");
                }
            }
            winners = tempPlayers;
        } while (hasChangesBeenMade && winners.Count > 1);

        return winners;
    }


    // -1 left wins, 0 tie, 1 right wins
    private static int CompareWinningHands(WinningHand? left, WinningHand? right)
    {
        if (left is null || right is null) throw new InternalPokerAlgoException($"Invariant violated: A passed winning hand argument is null.");
        
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
                if (leftCards[4].Rank > rightCards[4].Rank) return -1;
                else if (rightCards[4].Rank > leftCards[4].Rank) return 1;

                else return CompareKickers(new List<Card> { leftCards[0] }, new List<Card> { rightCards[0] });

            case HandType.FullHouse:
                if (leftCards[4].Rank > rightCards[4].Rank) return -1;
                else if (rightCards[4].Rank > leftCards[4].Rank) return 1;
                else if (leftCards[0].Rank > rightCards[0].Rank) return -1;
                else if (rightCards[0].Rank > leftCards[0].Rank) return 1;
                else return 0;

            case HandType.Flush:
                return CompareKickers(leftCards, rightCards);

            case HandType.Straight:
                return CompareKickers(leftCards, rightCards);

            case HandType.ThreeKind:
                if (leftCards[4].Rank > rightCards[4].Rank) return -1;
                else if (rightCards[4].Rank > leftCards[4].Rank) return 1;

                else return CompareKickers(leftCards.GetRange(0, 2), rightCards.GetRange(0, 2));

            case HandType.TwoPair:
                if (leftCards[4].Rank > rightCards[4].Rank) return -1;
                else if (rightCards[4].Rank > leftCards[4].Rank) return 1;
                if (leftCards[2].Rank > rightCards[2].Rank) return -1;
                else if (rightCards[2].Rank > leftCards[2].Rank) return 1;

                else return CompareKickers(new List<Card> { leftCards[0] }, new List<Card> { rightCards[0] });

            case HandType.Pair:
                if (leftCards[4].Rank > rightCards[4].Rank) return -1;
                else if (rightCards[4].Rank > leftCards[4].Rank) return 1;

                else return CompareKickers(leftCards.GetRange(0, 3), rightCards.GetRange(0, 3));

            case HandType.Nothing:
                return CompareKickers(leftCards, rightCards);

            default:
                throw new InternalPokerAlgoException("Invariant violated: switch defaulted. Was HandType enum changed?");
        }
    }

    // -1 left wins, 0 tie, 1 right wins
    private static int CompareKickers(List<Card> left, List<Card> right)
    {
        Helpers.DebugLogCards("Algo.CompareKickers() - Left", left);
        Helpers.DebugLogCards("Algo.CompareKickers() - Right", right);

        if (left.Count != right.Count)
        {
            throw new InternalPokerAlgoException($"Invariant violated: left.Count != right.Count." + "\nLeft: " + string.Join(' ', left) + "\nRight: " + string.Join(' ', right));
        }

        for (int i = left.Count - 1; i >= 0; i--)
        {
            if (left[i].Rank > right[i].Rank)
            {
                Helpers.DebugLog("Algo.CompareKickers() - Left Wins", 2);
                return -1;
            }
            else if (right[i].Rank > left[i].Rank)
            {
                Helpers.DebugLog("Algo.CompareKickers() - Right Wins", 2);
                return 1;
            }
        }
        Helpers.DebugLog("Algo.CompareKickers() - Tie", 2);

        return 0;
    }

}