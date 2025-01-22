using System.Diagnostics;

namespace PokerAlgo
{
    public static class Algo
    {
        public static int _debugVerbosity { get; set; } = 0; // * Verbosity Levels | 0 = Disabled | 1 = Progress Report | 2 = Everything

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
                if (_debugVerbosity > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("💭 Determining Hand for \'" + player.Name + "\'");
                    Console.ResetColor();
                }

                combinedCards = new()
                {
                    player.HoleCards.First,
                    player.HoleCards.Second
                };
                combinedCards.AddRange(communityCards);

                player.WinningHand = handEvaluator.GetWinningHand(combinedCards);
            }

            Player community = new("Community", new Card(0, CardSuit.Spades, true), new Card(0, CardSuit.Spades, true));

            if (_debugVerbosity > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n🃏 Determining Hand for Community");
                Console.ResetColor();
            }

            community.WinningHand = handEvaluator.GetCommunityWinningHand(communityCards);

            Helpers.DebugLog("\n--- 💭 Find Winners");

            List<Player> winners = DetermineWinners(players, community);

            Debug.Assert(winners.Count >= 1, "Algo.GetWinners() - winners.Count < 1.");

            return winners;
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

            Helpers.DebugLogPlayers("Algo.FindWinners() - Players after sorting by WinningHand.Type", players, community);

            Helpers.DebugLog("", 2);

            // ! If community is greater. 2 options:
            if(community.WinningHand.Type > players.ElementAt(0).WinningHand.Type)
            {
                // ! If community has best 5 cards then tie among all players
                if(_numberOfCardsOfHand[community.WinningHand.Type] == 5)
                {
                    Helpers.DebugLog("Algo.DetermineWinners() - Community Hand is Better | 5 Card Hand | Tie Among All Players\n", 1);

                    return players;
                }

                // ! If community has less than 5 best cards then tie break players.
                else
                {
                    Helpers.DebugLog($"Algo.DetermineWinners() - Community Hand is Better | {_numberOfCardsOfHand[community.WinningHand.Type]} Card Hand | Tie Break Players\n", 1);
                    List<Player> winners = BreakTieCommunityLessThanFiveCards(players, community);

                    return winners;
                }
            }

            // ! If community and player are the same. Tie break everything. If community wins it's a tie
            else if (community.WinningHand.Type == players.ElementAt(0).WinningHand.Type)
            {
                Helpers.DebugLog("Algo.DetermineWinners() - Community Hand is Equal | Tie Break Community + Players\n", 1);
                players.Add(community);
                List<Player> winners = BreakTies(players);

                if(winners.Count == 1 && winners.Contains(community))
                {
                    Helpers.DebugLog("Algo.DetermineWinners() - Community Wins | Everyone Ties\n", 1);
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
                Helpers.DebugLog("Algo.DetermineWinners() - Players Best Hand is Better | Tie Break Players Only\n", 1);
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
                    Helpers.DebugLog($"Algo.BreakTies() - " + (result == -1 ? winners[playerIndex].Name + " has better hand\n" : result == 1 ? winners[playerIndex + 1].Name + " has better hand\n" : "Players Tie"), 2);

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
                    Helpers.DebugLog($"Algo.BreakTieCommunityLessThanFiveCards() - {winners.ElementAt(i).Name} | Hand: {winners.ElementAt(i).WinningHand.Type}", 2);
                    Helpers.DebugLog($"Algo.BreakTieCommunityLessThanFiveCards() - {winners.ElementAt(i+1).Name} | Hand: {winners.ElementAt(i + 1).WinningHand.Type}", 2);

                    tempLeft = winners.ElementAt(i).WinningHand.Cards.ToList().Except(winningCards).ToList();
                    tempLeft = tempLeft.GetRange(tempLeft.Count - numberRemaining, numberRemaining);

                    tempRight = winners.ElementAt(i+1).WinningHand.Cards.ToList().Except(winningCards).ToList();
                    tempRight = tempRight.GetRange(tempRight.Count - numberRemaining, numberRemaining);

                    int result = CompareKickers(tempLeft, tempRight);
                    Helpers.DebugLog($"Algo.BreakTieCommunityLessThanFiveCards() - " + (result == -1 ? winners[i].Name + " has better hand\n" : result == 1 ? winners[i + 1].Name + " has better hand\n" : "Players Tie"), 2);

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


        // ! -1 left wins, 0 tie, 1 right wins
        private static int ComparePlayerHands(Player player1, Player player2)
        {
            if (player1.WinningHand is null || player2.WinningHand is null) throw new Exception("⛔ Algo.ComparePlayerHands(): A player's winning hand is null.");

            WinningHand left = player1.WinningHand;
            WinningHand right = player2.WinningHand;

            // * left.Type = HandType.RoyalFlush;
            // * right.Type = HandType.RoyalFlush;

            Helpers.DebugLog($"Algo.ComparePlayerHands() - {player1.Name} | Hand: {player1.WinningHand.Type}", 2);
            Helpers.DebugLog($"Algo.ComparePlayerHands() - {player2.Name} | Hand: {player2.WinningHand.Type}", 2);

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
                    throw new Exception("⛔ Algo.ComparePlayerHands(): Since only one player can have a royal flush it makes no sense that it would get to this point. "
                                        + "Also since my code does not give community winning hands to the players, no two players can have a royal flush.");

                case HandType.StraightFlush:
                    return CompareKickers(leftCards, rightCards);

                case HandType.FourKind:
                    if (leftCards.ElementAt(4).Rank > rightCards.ElementAt(4).Rank) return -1;
                    else if (rightCards.ElementAt(4).Rank > leftCards.ElementAt(4).Rank) return 1;
                    else return CompareKickers(new List<Card> { leftCards.ElementAt(0) }, new List<Card> { rightCards.ElementAt(0) });

                case HandType.FullHouse:
                    if (leftCards.ElementAt(0).Rank > rightCards.ElementAt(0).Rank) return -1;
                    else if (rightCards.ElementAt(0).Rank > leftCards.ElementAt(0).Rank) return 1;
                    else if (leftCards.ElementAt(3).Rank > rightCards.ElementAt(3).Rank) return -1;
                    else if (rightCards.ElementAt(3).Rank > leftCards.ElementAt(3).Rank) return 1;
                    else return 0;

                case HandType.Flush:
                    return CompareKickers(leftCards, rightCards);

                case HandType.Straight:
                    return CompareKickers(leftCards, rightCards);

                case HandType.ThreeKind:
                    if (leftCards.ElementAt(4).Rank > rightCards.ElementAt(4).Rank) return -1;
                    else if (rightCards.ElementAt(4).Rank > leftCards.ElementAt(4).Rank) return 1;
                    else return CompareKickers(leftCards.GetRange(0, 2), rightCards.GetRange(0, 2));

                case HandType.TwoPairs:
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
                    throw new Exception("⛔ Algo.ComparePlayerHands(): Switch defaulted.");
            }

            throw new NotImplementedException();
        }

        // ! -1 left wins, 0 tie, 1 right wins
        private static int CompareKickers(List<Card> left, List<Card> right)
        {
            Helpers.DebugLogCards("Algo.CompareKickers() - Left", left);
            Helpers.DebugLogCards("Algo.CompareKickers() - Right", right);

            if (left.Count != right.Count)
            {
                Console.WriteLine(string.Join(' ', left));
                Console.WriteLine(string.Join(' ', right));
                throw new Exception("⛔ Algo.CompareKickers(): left.Count != right.Count.");
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


        internal static void SortCardsByValue(List<Card> cards)
        {
            cards.Sort((x, y) => x.Rank.CompareTo(y.Rank));
        }

    }
}
