namespace PokerAlgo;
/// <summary>
/// Represents a player in a Texas Hold'em hand, including their name, hole cards, and evaluated winning hand.
/// </summary>
public class Player
{
	/// <summary>
	/// Gets the player's name.
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// Gets the player's hole cards as a <see cref="Pair"/>.
	/// </summary>
	public Pair HoleCards { get; protected set; }
	/// <summary>
	/// Gets or sets the player’s winning hand after evaluation, if any.
	/// </summary>
	public WinningHand? WinningHand { get; set; }
	/// <summary>
	/// Initializes a new instance of the <see cref="Player"/> class with a name and two hole cards.
	/// </summary>
	/// <param name="name">The player's display name.</param>
	/// <param name="first">The first hole card.</param>
	/// <param name="second">The second hole card.</param>
	public Player(string name, Card first, Card second)
	{
		this.Name = name;
		this.HoleCards = new Pair(first, second);

		HoleCards.First.IsPlayerCard = true;
		HoleCards.Second.IsPlayerCard = true;

		this.WinningHand = null;
	}

	public void NewHand(Card first, Card second)
	{
		first.IsPlayerCard = true;
		second.IsPlayerCard = true;
		HoleCards = new Pair(first, second);
	}

	public override string ToString()
	{
		return $"{Name}: {HoleCards.First} {HoleCards.Second}";
	}
}