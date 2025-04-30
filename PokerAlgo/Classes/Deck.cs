namespace PokerAlgo;
public class Deck
{
	private readonly List<Card> _cards;
	public int NextCardIndex { get; private set; }

	public Deck()
	{
		_cards = new List<Card>();
		NextCardIndex = 0;

		Create();

		Shuffle();
	}

	private void Create()
	{
		foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
		{
			for (int rank = 2; rank <= 14; rank++)
			{
				_cards.Add(new Card(rank, suit, false));
			}
		}
	}

	private void Shuffle()
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
			tempCard = _cards[currentIndex];
			_cards[currentIndex] = _cards[targetIndex];
			_cards[targetIndex] = tempCard;
		}
	}

	public void ResetDeck()
	{
		for (int i = 0; i < NextCardIndex; i++)
		{
			_cards[i].IsPlayerCard = false;
		}
		NextCardIndex = 0;

		Shuffle();
	}

	// Returns the first card, and then removes it from the deck
	public Card NextCard()
	{
		if (NextCardIndex >= _cards.Count) throw new DeckEmptyException("No more cards in the Deck");

		return _cards[NextCardIndex++];
	}

	public List<Card> NextCards(int numberOfCards)
	{
		if (numberOfCards < 1) throw new ArgumentOutOfRangeException(nameof(numberOfCards),"The number of cards passed must be greater than 0.");
		if (NextCardIndex >= _cards.Count) throw new DeckEmptyException("No more cards in the Deck");
		if (NextCardIndex - 1 + numberOfCards >= _cards.Count) throw new NotEnoughCardsInDeckException($"Cards Left: {_cards.Count - (NextCardIndex - 1)}. Cards requested: {numberOfCards}");

		List<Card> cardsToReturn = _cards.GetRange(NextCardIndex, numberOfCards);
		NextCardIndex += numberOfCards;

		return cardsToReturn;
	}

	public void RemoveCards(List<Card> cardsToRemove)
	{
		foreach (Card card in cardsToRemove)
		{
			int index = _cards.IndexOf(card);
			if (index == -1) throw new CardNotInDeckException($"Invariant violated: card to remove {card} was not found in deck.");

			if (index > NextCardIndex - 1)
			{
				NextCardIndex++;
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