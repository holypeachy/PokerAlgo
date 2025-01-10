namespace PokerAlgo
{ 
    class PlayerWinningObj
    {
        public Player Owner { get; set; }
        public List<Card> Cards { get; set; }

        public PlayerWinningObj(Player player, List<Card> cards)
        {
            this.Owner = player;
            this.Cards = cards;
        }

        public override string ToString()
        {
            return $"{Owner.Name}:\n\t{string.Join(' ', Cards)}";
        }
    }
}