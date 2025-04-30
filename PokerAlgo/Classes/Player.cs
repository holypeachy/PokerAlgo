namespace PokerAlgo;
public class Player
{
	public string Name { get; }
	public Pair HoleCards { get; protected set; }

	public WinningHand? WinningHand { get; set; }

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