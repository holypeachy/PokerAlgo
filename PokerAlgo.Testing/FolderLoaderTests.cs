namespace PokerAlgo.Testing;

public class FolderLoaderTests
{
    [Fact]
    public void Load_no_data_read_throws_IOException()
    {
        string path = @"C:/Users/Frank/Code/PokerAlgo/Resources/";
        IPreFlopDataLoader loader = new FolderLoader(path);

        Assert.Throws<IOException>(() => loader.Load());
    }
    
}