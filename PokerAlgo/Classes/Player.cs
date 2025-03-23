namespace PokerAlgo;
public class Player
{
	public string Name { get; }
	public Pair<Card, Card> HoleCards { get; }
	
	public WinningHand? WinningHand { get; set; }


	public Player(string name, Card first, Card second)
	{
		this.Name = name;
		this.HoleCards = new Pair<Card, Card>(first, second);
		
		this.HoleCards.First.IsPlayerCard = true;
		this.HoleCards.Second.IsPlayerCard = true;

		this.WinningHand = null;
	}

	public override string ToString()
	{
		return $"{Name}: {HoleCards.First} {HoleCards.Second}";
	}
	
	public void SortHand(){
		Card bufferCard;
		if(HoleCards.First.Rank > HoleCards.Second.Rank){
			bufferCard = HoleCards.Second;
			HoleCards.Second = HoleCards.First;
			HoleCards.First = bufferCard;
		}
	}
}