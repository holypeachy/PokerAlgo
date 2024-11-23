namespace PokerAlgo
{
    static class Algo
    {
        private static string[] _suits = { "Spades", "Clubs", "Hearts", "Diamonds" };

        public static void FindWinner(List<Player> players, List<Card> community)
        {
            // Lot of fancy code stuff
            // DeterminePlayerHands(players[0], community);
            Player testPlayer = new Player("Test",
            new Card(1, _suits[3]),
            new Card(10, _suits[3]));

            List<Card> testCom = new List<Card>(){
            new Card(5, _suits[2]),
            new Card(11, _suits[3]),
            new Card(12, _suits[3]),
            new Card(13, _suits[3]),
            new Card(7, _suits[2])};

            DeterminePlayerHands(testPlayer, testCom);
        }

        private static void DeterminePlayerHands(Player player, List<Card> community)
        {
            List<Card> cards = new();

            cards.Add(player.Hand.Item1);
            cards.Add(player.Hand.Item2);
            foreach (Card c in community)
            {
                cards.Add(c);
            }

            cards = cards.OrderBy(x => x.Value).ToList();

            // Console.ReadLine();
            // Console.Clear();
            foreach (Card c in cards)
            {
                Console.WriteLine($"{c}" + (c.IsPlayerCard ? "player" : "") );
            }

            FlushFinder(cards, player);
        }

        private static void FlushFinder(List<Card> cards, Player player){
            List<Card> currentWinnerHand = new();
            List<Card> flushCards = new();

            // ! Flush?
            bool isFlush = false;
            string flushSuit = "";

            // Do we have a flush?
            foreach (string currentSuit in _suits)
            {
                int count = 0;
                foreach (Card c in cards)
                {
                    if (c.Suit == currentSuit)
                    {
                        count++;
                    }
                }

                if (count >= 5)
                {
                    isFlush = true;
                    flushSuit = currentSuit;
                    break;
                }
            }

            if(!isFlush){
                return;
            }

            // Extract all flush cards
            Console.WriteLine("\nFlush Cards:");
            foreach (Card c in cards)
            {
                if(c.Suit == flushSuit){
                    flushCards.Add(c);
                    Console.WriteLine(c);
                }
            }

            // Do we have a royal flush?
            bool isRoyalFlush = true;
            bool[] royalMatches = { false, false, false, false, false };
            // ! Royal Flush?
            foreach (Card c in flushCards)
            {
                switch (c.Value)
                {
                    case 1:
                        royalMatches[0] = true;
                        currentWinnerHand.Add(c);
                        break;
                    case 10:
                        royalMatches[1] = true;
                        currentWinnerHand.Add(c);
                        break;
                    case 11:
                        royalMatches[2] = true;
                        currentWinnerHand.Add(c);
                        break;
                    case 12:
                        royalMatches[3] = true;
                        currentWinnerHand.Add(c);
                        break;
                    case 13:
                        royalMatches[4] = true;
                        currentWinnerHand.Add(c);
                        break;
                    default:
                        break;
                }
            }

            foreach (bool condition in royalMatches)
            {
                if (condition == false)
                {
                    isRoyalFlush = false;
                    break;
                }
            }

            bool isInHand = false;
            foreach (Card c in currentWinnerHand)
            {
                if (c.IsPlayerCard)
                {
                    isInHand = true;
                    break;
                }
            }

            bool isStraightFlush = false;

            if (isRoyalFlush && isInHand)
            {
                player.HighestScore = 9;
                WinningHand tempWinning = new(HandType.RoyalFlush);
                tempWinning.Cards = currentWinnerHand;
                player.WinningHands.Add(tempWinning);
                Console.Write("\nROYAL FLUSH: ");
                foreach (Card c in currentWinnerHand)
                {
                    Console.Write($"{c} ");
                }
            }
            // ! Straight Flush?
            else if(!isRoyalFlush){
                if(flushCards.Count == 5){

                }
                else if(flushCards.Count == 6){

                }
                else if (flushCards.Count == 7){

                }
            }
            // ! If not Royal Flush or Straight Flush. It's just a regular Flush
            else{

            }
        }
    
    }
}

// TODO: Cleanup
// TODO: Add helper function to check if a list of card contains any cards from player.
