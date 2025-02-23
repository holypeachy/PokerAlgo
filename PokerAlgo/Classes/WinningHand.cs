namespace PokerAlgo
{
    public class WinningHand
    {
        public HandType Type { get; }
        public List<Card> Cards { get; }

        public WinningHand(HandType type, List<Card> cards)
        {
            this.Type = type;
            this.Cards = cards;
        }

        public override string ToString()
        {
            return $"Type: {Type} " + $"| Cards: {string.Join(" ", Cards)}";
        }
    }
}