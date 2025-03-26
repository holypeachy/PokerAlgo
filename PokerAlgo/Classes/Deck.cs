namespace PokerAlgo;
public class Deck
{
	private readonly List<Card> _cards;
	private int _nextCardIndex;

	public Deck()
	{
		_cards = new List<Card>();
		_nextCardIndex = 0;

		CreateDeck();

		ShuffleDeck();
	}

	private void CreateDeck()
	{
		_cards.Clear();

		foreach ( CardSuit suit in Enum.GetValues( typeof(CardSuit) ) )
		{
			for (int rank = 2; rank <= 14; rank++)
			{
				_cards.Add(new Card(rank, suit, false));
			}
		}
	}

	private void ShuffleDeck()
	{
		Random rand = new();
		Card tempCard;
		int targetIndex;
		
		for (int currentIndex = 0; currentIndex < _cards.Count; currentIndex++)
		{
			targetIndex = rand.Next(_cards.Count);
			if(targetIndex == currentIndex){
				continue;
			}
			tempCard = _cards.ElementAt(currentIndex);
			_cards[currentIndex] = _cards.ElementAt(targetIndex);
			_cards[targetIndex] = tempCard;
		}
	}

	// public void PrintDeck()
	// {
	// 	foreach (Card card in _cards)
	// 	{
	// 		Console.WriteLine(card);
	// 	}
	// }

	public void ResetDeck()
	{
		for (int i = 0; i < _nextCardIndex; i++)
		{
			_cards[i].IsPlayerCard = false;
		}
		_nextCardIndex = 0;

		ShuffleDeck();
	}

	// Returns the first card, and then removes it from the deck
	public Card NextCard()
	{
		if (_nextCardIndex >= _cards.Count) throw new Exception("⛔ Deck.NextCard() - No More Cards in The Deck");

		return _cards.ElementAt(_nextCardIndex++);
	} 

	public void RemoveCards(List<Card> cardsToRemove)
	{
		_nextCardIndex += cardsToRemove.Count;
		foreach (Card card in cardsToRemove)
		{
			if(_cards.Remove(card) == false) throw new Exception($"⛔ Deck.RemoveCards() - Card to remove {card} was not found in deck.");
			_cards.Insert(0, card);
		}
	}
	
}