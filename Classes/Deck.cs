namespace PokerAlgo
{
    class Deck
    {
        public List<Card> Cards {get; set;}

        public Deck()
        {
            Cards = new List<Card>();

            CreateDeck();

            ShuffleDeck();
        }

        public void CreateDeck()
        {
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>() )
            {
                for (int value = 1; value <= 13; value++)
                {
                    Cards.Add(new Card(value, suit, false));
                }
            }
        }

        public void ShuffleDeck()
        {
            Random rand = new();
            Card tempCard;
            int randomNum;
            
            for (int target = 0; target < Cards.Count; target++)
            {
                randomNum = rand.Next(52);
                if(randomNum == target){
                    continue;
                }
                tempCard = Cards.ElementAt(target);
                Cards[target] = Cards.ElementAt(randomNum);
                Cards[randomNum] = tempCard;
            }
        }

        public void PrintDeck()
        {
            foreach (Card card in Cards)
            {
                Console.Write($"[{card.Value},{card.Suit}] \n");
            }
        }


        // Returns the first card, and then removes it from the deck
        public Card NextCard()
        {
            if(Cards.Count <= 0)
            {
                throw new Exception("No More Cards in The Deck");
            }

            Card cardToRemove = Cards.First();
            Cards.RemoveAt(0);
            return cardToRemove;
        } 
    }
}