namespace PokerAlgo;

public class FolderLoader : IPreFlopDataLoader
{
    private readonly string _folderPath;

    private Dictionary<(string holeCardsInNotation, int opponentCount), (double winChance, double tieChance)>? LookupTable = null;

    public FolderLoader(string folderPath)
    {
        _folderPath = folderPath;
    }

    Dictionary<(string holeCardsInNotation, int opponentCount), (double winChance, double tieChance)> IPreFlopDataLoader.Load()
    {
        if (LookupTable is null)
        {
            LookupTable = new();
            string[] files = Directory.GetFiles(_folderPath, "*.preflop");

            string fileName;
            string[] splitFileName;
            int numberOfOpponents;
            string[] fileData;
            string[] currentLine;

            foreach (string file in files)
            {
                fileName = Path.GetFileName(file);
                splitFileName = fileName.Split('_');

                if (splitFileName.Length != 2 || !int.TryParse(splitFileName[0], out numberOfOpponents))
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