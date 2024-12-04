namespace PokerAlgo
{
    class WinningHand
    {
        public HandType Type { get; set; }
        public List<Card> Cards { get; set; }

        public WinningHand(HandType type, List<Card> cards)
        {
            this.Type = type;
            this.Cards = cards;
        }

        public override string ToString()
        {
            return $"Type: {Type} | Cards: {string.Join(" ", Cards)}";
        }
    }
}