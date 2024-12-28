namespace PokerAlgo{

	class Player
	{
		public string Name { get; set; }
		public Pair<Card, Card> Hand { get; set; }
		
		public WinningHand? WinningHand { get; set; }
		public bool IsWinner { get; set; }


		public Player(string name, Card first, Card second)
		{
			this.Name = name;
			this.Hand = new Pair<Card, Card>(first, second);
			
			this.Hand.First.IsPlayerCard = true;
			this.Hand.Second.IsPlayerCard = true;

			this.WinningHand = null;
			this.IsWinner = false;
		}

		public override string ToString()
		{
			return $"{Name}: {Hand.First} {Hand.Second}";
		}
		
		public void SortHand(){
			Card bufferCard;
			if(Hand.First.Value > Hand.Second.Value){
				bufferCard = Hand.Second;
				Hand.Second = Hand.First;
				Hand.First = bufferCard;
			}
		}
	}
}