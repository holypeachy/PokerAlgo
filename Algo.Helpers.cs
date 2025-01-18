namespace PokerAlgo
{
    static partial class Algo
    {
        // * Helper Methods
        // ! -1 left wins, 0 tie, 1 right wins
        private static int ComparePlayerHands(Player player1, Player player2)
        {
            if (player1.WinningHand is null || player2.WinningHand is null) throw new Exception("⛔ Algo.ComparePlayerHands(): A player's winning hand is null.");

            WinningHand left = player1.WinningHand;
            WinningHand right = player2.WinningHand;

            // * left.Type = HandType.RoyalFlush;
            // * right.Type = HandType.RoyalFlush;

            DebugLog($"Algo.ComparePlayerHands() - {player1.Name} | Hand: {player1.WinningHand.Type}", 2);
            DebugLog($"Algo.ComparePlayerHands() - {player2.Name} | Hand: {player2.WinningHand.Type}", 2);

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
                    throw new Exception("⛔ Algo.ComparePlayerHands(): Switch defaulted.");
            }

            throw new NotImplementedException();
        }

        // ! -1 left wins, 0 tie, 1 right wins
        private static int CompareKickers(List<Card> left, List<Card> right)
        {
            DebugLogCards("Algo.CompareKickers() - Left", left);
            DebugLogCards("Algo.CompareKickers() - Right", right);

            if (left.Count != right.Count)
            {
                Console.WriteLine(string.Join(' ', left));
                Console.WriteLine(string.Join(' ', right));
                throw new Exception("⛔ Algo.CompareKickers(): left.Count != right.Count.");
            }

            for (int i = left.Count - 1; i >= 0; i--)
            {
                if (left.ElementAt(i).Value > right.ElementAt(i).Value)
                {
                    DebugLog("Algo.CompareKickers() - Left Wins", 2);
                    return -1;
                }
                else if (right.ElementAt(i).Value > left.ElementAt(i).Value)
                {
                    DebugLog("Algo.CompareKickers() - Right Wins", 2);
                    return 1;
                }
            }
            DebugLog("Algo.CompareKickers() - Tie", 2);

            return 0;
        }


        private static List<Card> CompleteWinningHand(List<Card> winningCards, List<Card> allCards)
        {
            List<Card> completeHand = winningCards.ToList();
            int neededNumberOfCards = 5 - winningCards.Count;
            List<Card> remainingCards = allCards.Except(winningCards).ToList();
            DebugLog("", 2);
            DebugLogCards("Algo.CompleteWinningHand() - remainingCards", remainingCards);

            if (neededNumberOfCards < 1)
            {
                throw new Exception("⛔ Algo.CompleteWinningHand(): neededNumberOfCards is less than 1. Something is very wrong.");
            }

            while (neededNumberOfCards > 0)
            {
                completeHand.Insert(0, remainingCards.ElementAt(remainingCards.Count - 1));
                remainingCards.RemoveAt(remainingCards.Count - 1);
                neededNumberOfCards--;
            }

            if (completeHand.Count != 5)
            {
                throw new Exception("⛔ Algo.CompleteWinningHand(): completeHand.Count != 5. Logic is wrong.");
            }
            return completeHand;
        }

        private static List<Card> GetBestFiveCards(List<Card> cards)
        {
            if (cards.Count >= 5)
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
                    Console.ForegroundColor = ConsoleColor.Black;
                    // Console.ForegroundColor = c.Suit == CardSuit.Spades || c.Suit == CardSuit.Clubs ? ConsoleColor.Black : ConsoleColor.Red;
                    Console.Write($"{c} ");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        private static void AddLowAces(List<Card> cards)
        {
            List<Card> acesToAdd = new();
            foreach (Card c in cards)
            {
                if (c.Value == 14)
                {
                    acesToAdd.Add(new Card(1, c.Suit, c.IsPlayerCard));
                }
            }
            cards.InsertRange(0, acesToAdd);
        }

        private static List<Card> RemoveLowAces(List<Card> cards)
        {
            return cards.Where(c => c.Value != 1).ToList();
        }

        private static void SortCardsByValue(List<Card> cards)
        {
            cards.Sort((x, y) => x.Value.CompareTo(y.Value));
        }

        private static bool HasPlayerCard(List<Card> cards)
        {
            foreach (Card c in cards)
            {
                if (c.IsPlayerCard)
                {
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

        private static bool IsSameSuit(List<Card> cards)
        {
            CardSuit suit = cards.ElementAt(0).Suit;
            foreach (Card c in cards)
            {
                if (c.Suit != suit)
                {
                    return false; ;
                }
            }
            return true;
        }

    }
}