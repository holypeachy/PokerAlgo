namespace PokerAlgo;

public interface IPreFlopDataLoader
{
    public Dictionary<(string holeCardsInNotation, int opponentCount), (double winChance, double tieChance)> Load();
}