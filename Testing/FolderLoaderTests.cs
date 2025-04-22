namespace Testing;

public class FolderLoaderTests
{
    [Fact]
    public void Load_Throws_When_DirectoryIsMissingOrInvalid()
    {
        string path = @"C:/Users/Frank/Code/PokerAlgo/Resources/";
        IPreFlopDataLoader loader = new FolderLoader(path);

        Assert.Throws<IOException>(() => loader.Load());
    }
    
}