namespace PokerAlgo;
public class HandEvaluator
{
    private WinningHand? _tempBestHand;


    public HandEvaluator()
    {
        _tempBestHand = null;
    }


    public WinningHand GetWinningHand(List<Card> combinedCards)
    {
        Guards.ArgsGetWinningHand(combinedCards);

        _tempBestHand = null;
        List<Card> cardsCopy = combinedCards.ToList();

        SortCardsByValue(cardsCopy);

        Helpers.DebugLogCards("HandEvaluator.GetWinningHand() - All Cards", cardsCopy);

        EvaluateHand(cardsCopy);

        if (_tempBestHand is null) throw new InternalPokerAlgoException("Invariant violated: _tempBestHand should never be null before returning. No winning hand was found.");

        return _tempBestHand;
    }
    
    public WinningHand GetWinningHand(Pair playerHoleCards, List<Card> communityCards)
    {
        List<Card> cards = new()
        {
            playerHoleCards.First,
            playerHoleCards.Second,
        };

        cards.AddRange(communityCards);

        Guards.ArgsGetWinningHand(cards);

        _tempBestHand = null;
        List<Card> cardsCopy = cards.ToList();

        SortCardsByValue(cardsCopy);

        Helpers.DebugLogCards("HandEvaluator.GetWinningHand() - All Cards", cardsCopy);

        EvaluateHand(cardsCopy);

        if (_tempBestHand is null) throw new InternalPokerAlgoException("Invariant violated: _tempBestHand should never be null before returning. No winning hand was found.");

        return _tempBestHand;
    }

    private void EvaluateHand(List<Card> cards)
    {
        List<Card> flushCards = cards.GroupBy(card => card.Suit)
        .Where(group => group.Count() >= 5)
        .SelectMany(group => group).ToList();

        List<Card> fourKind = cards.GroupBy(card => card.Rank)
        .Where(group => group.Count() == 4).SelectMany(group => group).ToList();

        List<Card> threeKinds = cards.GroupBy(card => card.Rank)
        .Where(group => group.Count() == 3)
        .SelectMany(group => group).ToList();

        SortCardsByValue(threeKinds);

        List<Card> pairs = cards.GroupBy(card => card.Rank)
        .Where(group => group.Count() == 2)
        .SelectMany(group => group).ToList();

        SortCardsByValue(pairs);


        Helpers.DebugLogCards("HandEvaluator.EvaluateHand() - Flush Cards", flushCards);
        Helpers.DebugLogCards("HandEvaluator.EvaluateHand() - Four Kind Cards", fourKind);
        Helpers.DebugLogCards("HandEvaluator.EvaluateHand() - Three Kind Cards", threeKinds);
        Helpers.DebugLogCards("HandEvaluator.EvaluateHand() - Pair Cards", pairs);


        List<Card> bestFive = new();

        // ! Royal Flush
        if(flushCards.Count >= 5)
        {
            bestFive = GetBestFiveCards(flushCards);
            if (bestFive[0].Rank == 10 && HasConsecutiveValues(bestFive))
            {
                SetWinningHand(HandType.RoyalFlush, bestFive);
                return;
            }

            // ! Straight Flush
            AddLowAces(flushCards);
            for (int i = flushCards.Count - 5; i >= 0; i--)
            {
                bestFive = flushCards.GetRange(i, 5);
                if (HasConsecutiveValues(bestFive))
                {
                    SetWinningHand(HandType.StraightFlush, bestFive);
                    return;
                }
            }
            flushCards = RemoveLowAces(flushCards);
        }

        // ! Four Kind
        if (fourKind.Count == 4)
        {
            SetWinningHand(HandType.FourKind, CompleteWinningHand(fourKind, cards));
            return;
        }

        // ! Two, 3 of a kind. Full House
        if (threeKinds.Count == 6)
        {
            List<Card> top3 = threeKinds.GetRange(3, 3);
            List<Card> bottom3 = threeKinds.GetRange(0, 3);
            List<Card> fullHouse = new();

            if (bottom3[0].IsPlayerCard && bottom3[1].IsPlayerCard)
            {
                fullHouse.AddRange(bottom3.GetRange(0, 2));
            }
            else if (bottom3[1].IsPlayerCard && bottom3[2].IsPlayerCard)
            {
                fullHouse.AddRange(bottom3.GetRange(1, 2));
            }
            else if(bottom3[0].IsPlayerCard && bottom3[2].IsPlayerCard)
            {
                fullHouse.Add(bottom3[0]);
                fullHouse.Add(bottom3[2]);
            }
            else
            {
                fullHouse.AddRange(bottom3.GetRange(1, 2));
            }

            fullHouse.AddRange(top3);

            SetWinningHand(HandType.FullHouse, fullHouse);
            return;
        }

        // ! One, 3 of a kind and 1 or 2 pairs. Full House
        else if (threeKinds.Count == 3 && pairs.Count >= 2)
        {
            List<Card> topPair = pairs.GetRange(pairs.Count - 2, 2);
            List<Card> fullHouse = new();

            fullHouse.AddRange(topPair);
            fullHouse.AddRange(threeKinds);

            SetWinningHand(HandType.FullHouse, fullHouse);
            return;
        }

        // ! Standard Flush
        if (flushCards.Count >= 5)
        {
            bestFive = flushCards.GetRange(flushCards.Count - 5, 5);
            SetWinningHand(HandType.Flush, bestFive);
            return;
        }

        // ! Straight
        List<Card> tempCards = cards.ToList();
        AddLowAces(tempCards);

        //  Removes duplicates
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

        Helpers.DebugLogCards("HandEvaluator.EvaluateHand() - Without Duplicates", tempCards);

        for (int i = tempCards.Count - 5; i >= 0; i--)
        {
            bestFive = tempCards.GetRange(i, 5);
            if (HasConsecutiveValues(bestFive))
            {

                SetWinningHand(HandType.Straight, bestFive);
                return;
            }
        }

        // ! Three of a kind
        if (threeKinds.Count == 3)
        {
            SetWinningHand(HandType.ThreeKind, CompleteWinningHand(threeKinds, cards));
            return;
        }

        // ! 2 Pairs or more
        else if (pairs.Count >= 4)
        {
            List<Card> topPair = pairs.GetRange(pairs.Count - 2, 2);
            List<Card> bottomPair = pairs.GetRange(pairs.Count - 4, 2);

            List<Card> twoPairs = new();

            twoPairs.AddRange(bottomPair);
            twoPairs.AddRange(topPair);

            SetWinningHand(HandType.TwoPair, CompleteWinningHand(twoPairs, cards));
            return;
        }

        // ! 1 Pair
        else if (pairs.Count == 2)
        {
            SetWinningHand(HandType.Pair, CompleteWinningHand(pairs, cards));
            return;
        }

        SetWinningHand(HandType.Nothing, CompleteWinningHand(new List<Card>(), cards));
    }


    private static List<Card> CompleteWinningHand(List<Card> winningCards, List<Card> allCards)
    {
        List<Card> completeHand = winningCards.ToList();
        int neededNumberOfCards = 5 - winningCards.Count;
        List<Card> remainingCards = allCards.Except(winningCards).ToList();
        Helpers.DebugLogCards("HandEvaluator.CompleteWinningHand() - remainingCards", remainingCards);

        if (neededNumberOfCards < 1) throw new InternalPokerAlgoException("Invariant violation: neededNumberOfCards is less than 1");


        while (neededNumberOfCards > 0)
        {
            completeHand.Insert(0, remainingCards[remainingCards.Count - 1]);
            remainingCards.RemoveAt(remainingCards.Count - 1);
            neededNumberOfCards--;
        }

        if (completeHand.Count != 5)
        {
            throw new InternalPokerAlgoException("Invariant violation: completeHand.Count != 5. Logic is wrong.");
        }
        return completeHand;
    }

    private static List<Card> GetBestFiveCards(List<Card> cards)
    {
        if(cards.Count < 5) throw new InternalPokerAlgoException("Invariant violation: list of cards passed has less than 5 cards.");
        
        return cards.GetRange(cards.Count - 5, 5);
    }

    private void SetWinningHand(HandType handType, List<Card> cards)
    {
        _tempBestHand = new WinningHand(handType, cards);

        Helpers.DebugLogWinningHand(handType, cards);
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
            }
        }
        return true;
    }

}