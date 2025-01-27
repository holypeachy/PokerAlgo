namespace PokerAlgo
{
	public class Deck
	{
		private List<Card> Cards {get; set;}

		public Deck()
		{
			Cards = new List<Card>();

			CreateDeck();

			ShuffleDeck();
			ShuffleDeck();
		}

		private void CreateDeck()
		{
			Cards = new List<Card>();

			foreach ( CardSuit suit in Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>() )
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
			int targetIndex;
			
			for (int currentIndex = 0; currentIndex < Cards.Count; currentIndex++)
			{
				targetIndex = rand.Next(Cards.Count);
				if(targetIndex == currentIndex){
					continue;
				}
				tempCard = Cards.ElementAt(currentIndex);
				Cards[currentIndex] = Cards.ElementAt(targetIndex);
				Cards[targetIndex] = tempCard;
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
			CreateDeck();

			ShuffleDeck();
			ShuffleDeck();
		}

		// Returns the first card, and then removes it from the deck
		public Card NextCard()
		{
			if(Cards.Count <= 0)
			{
				throw new Exception("⛔ Deck.NextCard() - No More Cards in The Deck");
			}

			Card cardOnTop = Cards.First();
			Cards.RemoveAt(0);
			return cardOnTop;
		} 
	
		public void RemoveCards(List<Card> cardsToRemove)
		{
			Cards = Cards.Except(cardsToRemove).ToList();
		}
		
	}
}