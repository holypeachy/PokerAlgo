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
			ShuffleDeck();
		}

		public void CreateDeck()
		{
			foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>() )
			{
				for (int rank = 2; rank <= 14; rank++)
				{
					Cards.Add(new Card(rank, suit, false));
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
				Console.WriteLine(card);
			}
		}

		public void ResetDeck()
		{
			Cards = new List<Card>();

			CreateDeck();

			ShuffleDeck();
			ShuffleDeck();
		}

		// Returns the first card, and then removes it from the deck
		public Card NextCard()
		{
			if(Cards.Count <= 0)
			{
				throw new Exception("Deck.NextCard() - No More Cards in The Deck");
			}

			Card cardToRemove = Cards.First();
			Cards.RemoveAt(0);
			return cardToRemove;
		} 
	}
}