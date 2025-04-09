namespace PokerAlgo;
/// <summary>
/// Loads precomputed preflop data from a folder containing `.preflop` files and builds a lookup table.
/// </summary>
public class FolderLoader : IPreFlopDataLoader
{
    private readonly string _folderPath;

    private Dictionary<(string holeCardsInNotation, int opponentCount), (double winChance, double tieChance)>? LookupTable = null;

    public FolderLoader(string folderPath)
    {
        _folderPath = folderPath;
    }

    /// <summary>
    /// Parses all `.preflop` files in the configured directory and builds a lookup table of preflop hand probabilities.
    /// </summary>
    /// <returns>A dictionary mapping hand notation and opponent count to win/tie probabilities.</returns>
    Dictionary<(string holeCardsInNotation, int opponentCount), (double winChance, double tieChance)> IPreFlopDataLoader.Load()
    {
        if (LookupTable is null)
        {
            LookupTable = new();
            string[] files = Directory.GetFiles(_folderPath, "*.preflop");

            string fileName;
            string[] splitFileName;
            string[] fileData;
            string[] currentLine;

            foreach (string file in files)
            {
                fileName = Path.GetFileName(file);
                splitFileName = fileName.Split('_');

                if (splitFileName.Length != 2 || !int.TryParse(splitFileName[0], out int numberOfOpponents))
                    throw new FormatException($"Unexpected file name format: \"{fileName}\"");

                fileData = File.ReadAllLines(file);

                for (int index = 0; index < fileData.Length; index++)
                {
                    currentLine = fileData[index].Split(' ');
                    LookupTable[(currentLine[0], numberOfOpponents)] = (double.Parse(currentLine[1]), double.Parse(currentLine[2]));
                }
            }

            if (LookupTable.Count == 0) throw new IOException($"No Data was read from \"{_folderPath}\"");
        }

        return LookupTable;
    }
    
}