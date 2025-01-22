using System.Diagnostics;

namespace PokerAlgo
{
    public class HandEvaluator
    {
        private static int _debugVerbosity = Algo._debugVerbosity;

        private WinningHand? _tempBestHand;


        public HandEvaluator()
        {
            _tempBestHand = null;
        }


        public WinningHand? GetWinningHand(List<Card> cards)
        {
            _tempBestHand = null;
            List<Card> cardsListCopy = cards.ToList();

            if (cards.Count < 5)
            {
                throw new Exception("⛔ HandEvaluator.GetWinningHand() - cards argument < 5");
            }

            SortCardsByValue(cardsListCopy);

            Helpers.DebugLogCards("HandEvaluator.GetWinningHand() - All Cards", cardsListCopy);

            EvaluateFlush(cardsListCopy);
            Helpers.DebugLog("", 2);
            EvaluateStraight(cardsListCopy);
            Helpers.DebugLog("", 2);
            EvaluateMultiples(cardsListCopy);

            Debug.Assert(_tempBestHand is not null, "HandEvaluator.GetWinningHand() - _tempBestHand should never be null before returning.");

            return _tempBestHand;
        }

        public WinningHand? GetCommunityWinningHand(List<Card> communityCards)
        {
            _tempBestHand = null;
            EvaluateCommunityWinningHand(communityCards);

            Debug.Assert(_tempBestHand is not null, "HandEvaluator.GetCommunityWinningHand() -  _tempBestHand should never be null before returning.");

            return _tempBestHand;
        }


        private void EvaluateFlush(List<Card> cards)
        {
            List<Card> flushCards = cards.GroupBy(card => card.Suit)
            .Where(group => group.Count() >= 5)
            .SelectMany(group => group).ToList();

            if (flushCards.Count == 0)
            {
                Helpers.DebugLog("⚠️  HandEvaluator.EvaluateFlush() - No Flush", 2);
                return;
            }

            Helpers.DebugLogCards("HandEvaluator.EvaluateFlush() - Flush Cards", flushCards);

            List<Card> bestFive = new();

            // ! Royal Flush
            bestFive = GetBestFiveCards(flushCards);
            if (bestFive.ElementAt(0).Rank == 10 && HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive))
            {
                SetWinningHand(HandType.RoyalFlush, bestFive);
                return;
            }

            AddLowAces(flushCards);

            // ! Straight Flush
            for (int i = flushCards.Count - 5; i >= 0; i--)
            {
                bestFive = flushCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive))
                {
                    SetWinningHand(HandType.StraightFlush, bestFive);
                    return;
                }
            }

            flushCards = RemoveLowAces(flushCards);

            // ! Standard Flush
            if (flushCards.Count >= 5)
            {
                for (int i = flushCards.Count - 5; i >= 0; i--)
                {
                    bestFive = flushCards.GetRange(i, 5);
                    if (HasPlayerCard(bestFive))
                    {
                        SetWinningHand(HandType.Flush, bestFive);
                        return;
                    }
                }
            }

            Helpers.DebugLog("⚠️  HandEvaluator.EvaluateFlush() - No Player Flush", 2);
        }

        private void EvaluateStraight(List<Card> cards)
        {
            // ! If Player already has a winning hand, no need to execute this method
            if (_tempBestHand is not null && _tempBestHand.Type > HandType.Straight)
            {
                Helpers.DebugLog($"⚠️  HandEvaluator.EvaluateStraight() - Early return. Player already has a higher winning hand {_tempBestHand.Type}.", 2);
                return;
            }

            // Shallow Copy, no need to deep copy here
            List<Card> tempCards = cards.ToList();
            AddLowAces(tempCards);

            // ! Removes duplicates
            for (int i = tempCards.Count - 1; i > 0; i--)
            {
                if (tempCards[i].Rank == tempCards[i - 1].Rank)
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

            Helpers.DebugLogCards("HandEvaluator.EvaluateStraight() - Without Duplicates", tempCards);


            for (int i = tempCards.Count - 5; i >= 0; i--)
            {
                List<Card> bestFive = tempCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive) && HasPlayerCard(bestFive))
                {
                    Debug.Assert(!IsSameSuit(bestFive), "⛔ HandEvaluator.EvaluateStraight() - bestFive cannot be all the same suit because it would be a flush.");

                    SetWinningHand(HandType.Straight, bestFive);
                    return;
                }
            }

            // ! For Testing
            Helpers.DebugLog("⚠️  HandEvaluator.EvaluateStraight() - No Straight", 2);
        }

        private void EvaluateMultiples(List<Card> cards)
        {
            // ! If Player already has a winning hand, no need to execute this method
            if (_tempBestHand is not null && _tempBestHand.Type > HandType.FourKind)
            {
                Helpers.DebugLog($"⚠️  HandEvaluator.EvaluateMultiples() - Early return at start. Player already has winning hand: {_tempBestHand.Type}.", 2);
                return;
            }

            List<Card> duplicateCards = RemoveLowAces(cards); // ? Just in case

            duplicateCards = duplicateCards.GroupBy(card => card.Rank)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group).ToList();

            if (duplicateCards.Count == 0)
            {
                Helpers.DebugLog("⚠️  HandEvaluator.EvaluateMultiples() - No Multiple", 2);
                if (_tempBestHand is null) SetWinningHand(HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));
                return;
            }

            Helpers.DebugLogCards("HandEvaluator.EvaluateMultiples() - Duplicate Cards", duplicateCards);

            // ! Four of a Kind
            List<Card> fourKind = duplicateCards.GroupBy(card => card.Rank)
            .Where(group => group.Count() == 4).SelectMany(group => group).ToList();

            if (fourKind.Count == 4 && HasPlayerCard(duplicateCards))
            {
                SetWinningHand(HandType.FourKind, CompleteWinningHand(fourKind, cards));
                return;
            }
            else
            {
                Helpers.DebugLog("⚠️  HandEvaluator.EvaluateMultiples() - No FourKind", 2);
            }

            List<Card> threeKinds = duplicateCards.GroupBy(card => card.Rank)
            .Where(group => group.Count() == 3)
            .SelectMany(group => group).ToList();

            List<Card> pairs = duplicateCards.GroupBy(card => card.Rank)
            .Where(group => group.Count() == 2)
            .SelectMany(group => group).ToList();

            SortCardsByValue(threeKinds);
            SortCardsByValue(pairs);

            // * Possible Combinations
            // * 7 cards. 2x 3k | 1x 3k , 2x pair | 1x 3k , 1x pair | 3k | 3x pair | 2x pair | 1x pair

            // ! Two, 3 of a kind. Full House
            if (threeKinds.Count == 6)
            {
                List<Card> top3 = threeKinds.GetRange(3, 3);
                List<Card> bottom3 = threeKinds.GetRange(0, 3);

                List<Card> fullHouse = new();
                if (HasPlayerCard(top3) || HasPlayerCard(bottom3))
                {
                    fullHouse.AddRange(top3);
                    if (bottom3[0].IsPlayerCard && bottom3[1].IsPlayerCard)
                    {
                        fullHouse.AddRange(bottom3.GetRange(0, 2));
                    }
                    else if (bottom3[1].IsPlayerCard && bottom3[2].IsPlayerCard)
                    {
                        fullHouse.AddRange(bottom3.GetRange(1, 2));
                    }
                    else
                    {
                        fullHouse.AddRange(bottom3.GetRange(1, 2)); // ! Warning for testing, because the last 2 cards will be picked. EqualsValue???
                    }
                }
                else
                {
                    // ! Unecessary but should keep for now. It's 6 out of 7 cards. The player has to have at least 1.
                    throw new Exception("⛔ MultipleFinder() 2x, 3 of a kind. It's not possible for 2, 3Ks to exist and the player not have a card in.");
                }

                SetWinningHand(HandType.FullHouse, fullHouse);
                return;
            }

            // ! One, 3 of a kind and 2 pairs. Full House
            else if (threeKinds.Count == 3 && pairs.Count == 4)
            {
                List<Card> topPair = pairs.GetRange(2, 2);
                List<Card> bottomPair = pairs.GetRange(0, 2);
                List<Card> fullHouse = new();

                if (HasPlayerCard(threeKinds) || HasPlayerCard(topPair))
                {
                    fullHouse.AddRange(threeKinds);
                    fullHouse.AddRange(topPair);
                }
                else if (HasPlayerCard(bottomPair))
                {
                    fullHouse.AddRange(threeKinds);
                    fullHouse.AddRange(bottomPair);
                }
                else
                {
                    // ! Unecessary but should keep for now. It's 7 out of 7 cards. The player has to have at least 1.
                    throw new Exception("⛔ MultipleFinder() 1x, 3 of a kind, 2 pairs. It's not possible for 1x 3K and 2 pairs to exist and the player not have a card in.");
                }

                SetWinningHand(HandType.FullHouse, fullHouse);
                return;
            }

            // ! One,  3 of a kind and 1 pair. Full House
            else if (threeKinds.Count == 3 && pairs.Count == 2)
            {
                List<Card> fullHouse = new();
                fullHouse.AddRange(threeKinds);
                fullHouse.AddRange(pairs);

                if (HasPlayerCard(fullHouse))
                {
                    SetWinningHand(HandType.FullHouse, fullHouse);
                    return;
                }
            }

            if (_tempBestHand is not null && _tempBestHand.Type > HandType.ThreeKind)
            {
                Helpers.DebugLog($"\n⚠️  HandEvaluator.EvaluateMultiples() - Early return after Full House. Player already has winning hand: {_tempBestHand.Type}.", 2);
                return;
            }

            // ! Three of a kind
            if (threeKinds.Count == 3)
            {
                if (HasPlayerCard(threeKinds))
                {
                    SetWinningHand(HandType.ThreeKind, CompleteWinningHand(threeKinds, cards));
                }
            }

            // ! 3 Pairs
            else if (pairs.Count == 6)
            {
                List<Card> topPair = pairs.GetRange(4, 2);
                List<Card> midPair = pairs.GetRange(2, 2);
                List<Card> bottomPair = pairs.GetRange(0, 2);

                List<Card> twoPairs = new();

                if (HasPlayerCard(midPair))
                {
                    twoPairs.AddRange(midPair);
                }
                else if (HasPlayerCard(bottomPair))
                {
                    twoPairs.AddRange(bottomPair);
                }
                else if (HasPlayerCard(topPair))
                {
                    twoPairs.AddRange(midPair);
                }
                else
                {
                    // ! Unecessary but should keep for now. It's 6 out of 7 cards. The player has to have at least 1.
                    throw new Exception("⛔ MultipleFinder() 3 pairs. It's not possible for 3 pairs to exist and the player not have a card in.");
                }
                twoPairs.AddRange(topPair);

                SetWinningHand(HandType.TwoPairs, CompleteWinningHand(twoPairs, cards));
            }

            // ! 2 Pairs
            else if (pairs.Count == 4)
            {
                if (HasPlayerCard(pairs))
                {
                    SetWinningHand(HandType.TwoPairs, CompleteWinningHand(pairs, cards));
                }
            }

            // ! 1 Pair
            else if (pairs.Count == 2)
            {
                if (HasPlayerCard(pairs))
                {
                    SetWinningHand(HandType.Pair, CompleteWinningHand(pairs, cards));
                }
            }

            // ! Nothing
            else
            {
                Helpers.DebugLog("⚠️  HandEvaluator.EvaluateMultiples() - No Multiple", 2);
                if (_tempBestHand is null) SetWinningHand(HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));
            }

            if (_tempBestHand is null) SetWinningHand(HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));
        }


        private void EvaluateCommunityWinningHand(List<Card> communityCards)
        {
            if (communityCards.Count < 5)
            {
                throw new Exception("HandEvaluator.DetermineCommunityWinningHand() - communityCards.Count < 5");
            }

            // Shallow copy for ease
            List<Card> cards = communityCards.ToList();
            SortCardsByValue(cards);

            Helpers.DebugLogCards("Algo.DetermineCommunityWinningHand() - Sorted Community Cards", cards);

            List<Card> bestFive = new();

            if (!IsSameSuit(cards))
            {
                Helpers.DebugLog("⚠️  Algo.DetermineCommunityWinningHand() - No Flush\n", 2);
            }
            else
            {
                // ! Royal Flush
                bestFive = GetBestFiveCards(cards);
                if (bestFive.ElementAt(0).Rank == 10 && HasConsecutiveValues(bestFive))
                {
                    SetWinningHand(HandType.RoyalFlush, bestFive);
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
                        SetWinningHand(HandType.StraightFlush, bestFive);
                        return;
                    }
                }
                cards = RemoveLowAces(cards);

                // ! Standard Flush
                SetWinningHand(HandType.Flush, cards);
                return;
            }

            List<Card> tempCards = cards.ToList();

            // ! Removes duplicates
            for (int i = tempCards.Count - 1; i > 0; i--)
            {
                if (tempCards[i].Rank == tempCards[i - 1].Rank)
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

            Helpers.DebugLogCards("Algo.DetermineCommunityWinningHand() - Without Duplicates", tempCards);
            // ! Straight
            AddLowAces(tempCards);
            if (tempCards.Count >= 5)
            {
                for (int i = tempCards.Count - 5; i >= 0; i--)
                {
                    bestFive = tempCards.GetRange(i, 5);
                    if (HasConsecutiveValues(bestFive) && !IsSameSuit(bestFive))
                    {
                        SetWinningHand(HandType.Straight, bestFive);
                        return;
                    }
                }
            }
            tempCards = RemoveLowAces(tempCards);

            Helpers.DebugLog("⚠️  Algo.DetermineCommunityWinningHand() - No Straight\n", 2);

            List<Card> duplicateCards = cards.GroupBy(card => card.Rank)
                            .Where(group => group.Count() > 1)
                            .SelectMany(group => group).ToList();

            if (duplicateCards.Count == 0)
            {
                Helpers.DebugLog("⚠️  Algo.DetermineCommunityWinningHand() - No Multiple\n", 2);
                SetWinningHand(HandType.Nothing, cards);
                return;
            }

            // ! Four of a Kind
            List<Card> fourKind = duplicateCards.GroupBy(card => card.Rank)
            .Where(group => group.Count() == 4).SelectMany(group => group).ToList();

            if (fourKind.Count == 4)
            {
                SetWinningHand(HandType.FourKind, CompleteWinningHand(fourKind, cards));
                return;
            }

            Helpers.DebugLog("⚠️  Algo.DetermineCommunityWinningHand() - No FourKind", 2);


            List<Card> threeKinds = duplicateCards.GroupBy(card => card.Rank)
            .Where(group => group.Count() == 3)
            .SelectMany(group => group).ToList();

            List<Card> pairs = duplicateCards.GroupBy(card => card.Rank)
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

                SetWinningHand(HandType.FullHouse, fullHouse);
            }

            // ! Three of a kind
            else if (threeKinds.Count == 3)
            {
                SetWinningHand(HandType.ThreeKind, CompleteWinningHand(threeKinds, cards));
            }

            // ! 2 Pairs
            else if (pairs.Count == 4)
            {
                SetWinningHand(HandType.TwoPairs, CompleteWinningHand(pairs, cards));
            }

            // ! 1 Pair
            else if (pairs.Count == 2)
            {
                SetWinningHand(HandType.Pair, CompleteWinningHand(pairs, cards));
            }
            else
            {
                SetWinningHand(HandType.Nothing, cards);
            }

            return;
        }


        private static List<Card> CompleteWinningHand(List<Card> winningCards, List<Card> allCards)
        {
            List<Card> completeHand = winningCards.ToList();
            int neededNumberOfCards = 5 - winningCards.Count;
            List<Card> remainingCards = allCards.Except(winningCards).ToList();
            Helpers.DebugLog("", 2);
            Helpers.DebugLogCards("HandEvaluator.CompleteWinningHand() - remainingCards", remainingCards);

            if (neededNumberOfCards < 1)
            {
                throw new Exception("⛔ HandEvaluator.CompleteWinningHand(): neededNumberOfCards is less than 1. Something is very wrong.");
            }

            while (neededNumberOfCards > 0)
            {
                completeHand.Insert(0, remainingCards.ElementAt(remainingCards.Count - 1));
                remainingCards.RemoveAt(remainingCards.Count - 1);
                neededNumberOfCards--;
            }

            if (completeHand.Count != 5)
            {
                throw new Exception("⛔ HandEvaluator.CompleteWinningHand(): completeHand.Count != 5. Logic is wrong.");
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
                throw new Exception("⛔ HandEvaluator.GetBestFiveCards(): The List<Card> passed has less than 5 cards.");
            }
        }

        private void SetWinningHand(HandType handType, List<Card> cards)
        {
            _tempBestHand = new WinningHand(handType, cards);

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
                if (c.Rank == 14)
                {
                    acesToAdd.Add(new Card(1, c.Suit, c.IsPlayerCard));
                }
            }
            cards.InsertRange(0, acesToAdd);
        }

        private static List<Card> RemoveLowAces(List<Card> cards)
        {
            return cards.Where(c => c.Rank != 1).ToList();
        }

        private static void SortCardsByValue(List<Card> cards)
        {
            cards.Sort((x, y) => x.Rank.CompareTo(y.Rank));
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
                    startingValue = cards[i].Rank;
                }
                else
                {
                    if (cards[i].Rank != ++startingValue)
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