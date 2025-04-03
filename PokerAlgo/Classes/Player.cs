namespace PokerAlgo;
public class Player
{
	public string Name { get; }
	public Pair HoleCards { get; }

	public WinningHand? WinningHand { get; set; }


	public Player(string name, Card first, Card second)
	{
		this.Name = name;
		this.HoleCards = new Pair(first, second);

		this.HoleCards.First.IsPlayerCard = true;
		this.HoleCards.Second.IsPlayerCard = true;

		this.WinningHand = null;
	}

	public override string ToString()
	{
		return $"{Name}: {HoleCards.First} {HoleCards.Second}";
	}
	
}