namespace PokerAlgo;
/// <summary>
/// Defines a contract for loading precomputed preflop data used in win/tie chance lookup.
/// </summary>
public interface IPreFlopDataLoader
{
    public Dictionary<(string holeCardsInNotation, int opponentCount), (double winChance, double tieChance)> Load();
}