namespace PokerAlgo;
/// <summary>
/// Represents a full 52-card poker deck with support for shuffling, drawing, and card removal.
/// </summary>
public class Deck
{
	private readonly List<Card> _cards;
	private int _nextCardIndex;

	/// <summary>
	/// Gets the index of the next card to be drawn from the deck.
	/// </summary>
	public int NextCardIndex { get { return _nextCardIndex; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="Deck"/> class.
	/// </summary>
	public Deck()
	{
		_cards = new List<Card>();
		_nextCardIndex = 0;

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

	/// <summary>
	/// Resets the deck by clearing the drawn cards, resetting player flags, and reshuffling.
	/// </summary>
	public void ResetDeck()
	{
		for (int i = 0; i < _nextCardIndex; i++)
		{
			_cards[i].IsPlayerCard = false;
		}
		_nextCardIndex = 0;

		Shuffle();
	}

	/// <summary>
	/// Draws and returns the next card from the deck.
	/// Throws if the deck is empty.
	/// </summary>
	/// <returns>The next <see cref="Card"/> in the deck.</returns>
	/// <exception cref="DeckEmptyException"></exception>
	public Card NextCard()
	{
		if (_nextCardIndex >= _cards.Count) throw new DeckEmptyException("No More Cards in The Deck");

		return _cards[_nextCardIndex++];
	}
	/// <summary>
	/// Draws and returns a specified number of cards from the deck.
	/// Throws if the request is invalid or if there aren’t enough cards.
	/// </summary>
	/// <param name="numberOfCards">The number of cards to retrieve from the deck.</param>
	/// <returns>A List of <see cref="Card"/>s.</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <exception cref="DeckEmptyException"></exception>
	/// <exception cref="NotEnoughCardsInDeckException"></exception>
	public List<Card> NextCards(int numberOfCards)
	{
		if (numberOfCards < 1) throw new ArgumentOutOfRangeException(nameof(numberOfCards),"The number of cards passed must be greater than 0.");
		if (_nextCardIndex >= _cards.Count) throw new DeckEmptyException("No More Cards in The Deck");
		if (_nextCardIndex - 1 + numberOfCards >= _cards.Count) throw new NotEnoughCardsInDeckException($"Cards Left: {_cards.Count - (_nextCardIndex - 1)}. Cards Requested: {numberOfCards}");

		List<Card> cardsToReturn = _cards.GetRange(_nextCardIndex, numberOfCards);
		_nextCardIndex += numberOfCards;

		return cardsToReturn;
	}
	/// <summary>
	/// Removes a list of cards from the deck, adjusting internal state accordingly.
	/// Throws if a card is not found in the deck.
	/// </summary>
	/// <param name="cardsToRemove">The list of cards you would like to remove from the deck.</param>
	/// <exception cref="CardNotInDeckException"></exception>
	public void RemoveCards(List<Card> cardsToRemove)
	{
		foreach (Card card in cardsToRemove)
		{
			int index = _cards.IndexOf(card);
			if (index == -1) throw new CardNotInDeckException($"Invariant violated: card to remove {card} was not found in deck.");

			if (index > _nextCardIndex - 1)
			{
				_nextCardIndex++;
				_cards.RemoveAt(index);
				_cards.Insert(0, card);
			}
		}
	}
	/// <summary>
	/// Returns a deep copy of the entire list of cards currently in the deck.
	/// This includes drawn and remaining cards.
	/// </summary>
	/// <returns>A List of cards.</returns>
	public List<Card> GetCopyOfListOfCards()
	{
		return _cards.Select(c => new Card(c)).ToList();
	}

}