namespace PokerAlgo;
public class Deck
{
	private readonly List<Card> _cards;
	private int _nextCardIndex;

	public int NextCardIndex { get { return _nextCardIndex; } }

	public Deck()
	{
		_cards = new List<Card>();
		_nextCardIndex = 0;

		CreateDeck();

		ShuffleDeck();
	}

	private void CreateDeck()
	{
		foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
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
			if (targetIndex == currentIndex)
			{
				continue;
			}
			tempCard = _cards.ElementAt(currentIndex);
			_cards[currentIndex] = _cards.ElementAt(targetIndex);
			_cards[targetIndex] = tempCard;
		}
	}

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
		if (_nextCardIndex >= _cards.Count) throw new DeckEmptyException("No More Cards in The Deck");

		return _cards.ElementAt(_nextCardIndex++);
	}

	public List<Card> NextCards(int numberOfCards)
	{
		if (numberOfCards < 1) throw new ArgumentOutOfRangeException("The number of cards passed must be greater than 0.");
		if (_nextCardIndex >= _cards.Count) throw new DeckEmptyException("No More Cards in The Deck");
		if (_nextCardIndex - 1 + numberOfCards >= _cards.Count) throw new NotEnoughCardsInDeckException($"Cards Left: {_cards.Count - (_nextCardIndex - 1)}. Cards Requested: {numberOfCards}");

		List<Card> cardsToReturn = _cards.GetRange(_nextCardIndex, numberOfCards);
		_nextCardIndex += numberOfCards;

		return cardsToReturn;
	}

	public void RemoveCards(List<Card> cardsToRemove)
	{
		foreach (Card card in cardsToRemove)
		{
			int index = _cards.IndexOf(card);
			if (index == -1) throw new CardNotInDeckException($"Internal error: unreachable code path. Card to remove {card} was not found in deck.");

			if (index > _nextCardIndex - 1)
			{
				_nextCardIndex++;
				_cards.RemoveAt(index);
				_cards.Insert(0, card);
			}
		}
	}

	public List<Card> GetCopyOfListOfCards()
	{
		return _cards.Select(c => new Card(c)).ToList();
	}

}