namespace PokerAlgo.Testing;

public class ChanceCalculatorTests
{
    public string pathToPreFlopDirectory = @"C:/Users/Frank/Code/PokerAlgo/Resources/preflop_data/";

    [Fact]
    public void GetWinningChanceSim_Throws_When_CommunityLessThanThree()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 2, 500));
    }

    [Fact]
    public void GetWinningChanceSim_Throws_When_CommunityMoreThanFive()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(6);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 2, 500));
    }

    [Fact]
    public void GetWinningChanceSim_Throws_When_OpponentsIsZero()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 0, 500));
    }

    [Fact]
    public void GetWinningChanceSim_Throws_When_SimCountIsTooLow()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 4, 10));
    }

    [Fact]
    public void GetWinningChanceSim_Returns_When_ValidInput()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 4, 500);
    }



    [Fact]
    public void GetWinningChancePreFlopSim_Throws_When_OpponentsIsZero()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChancePreFlopSim(playerCards, 0, 500));
    }

    [Fact]
    public void GetWinningChancePreFlopSim_Throws_When_SimCountIsTooLow()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChancePreFlopSim(playerCards, 0, 99));
    }


    [Fact]
    public void GetPreFlopChen_Returns12_For_AKs()
    {
        Pair holeCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(12);
    }

    [Fact]
    public void GetPreFlopChen_Returns10_For_TTo()
    {
        Pair holeCards = new(new Card(10, CardSuit.Spades, true), new Card(10, CardSuit.Hearts, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(10);
    }

    [Fact]
    public void GetPreFlopChen_Returns6_For_57s()
    {
        Pair holeCards = new(new Card(5, CardSuit.Spades, true), new Card(7, CardSuit.Spades, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(6);
    }

    [Fact]
    public void GetPreFlopChen_ReturnsNeg1_For_27o()
    {
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(7, CardSuit.Hearts, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(-1);
    }

    [Fact]
    public void GetPreFlopChen_Returns20_For_AAo()
    {
        Pair holeCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Hearts, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(20);
    }

    [Fact]
    public void GetPreFlopChen_Throws_When_DuplicateAA()
    {
        Pair holeCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Spades, true));

        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetPreFlopChen(holeCards));
    }


    [Fact]
    public void GetWinningChancePreFlopLookUp_Returns557242_For_AAo_4Opponents()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Clubs, true));
        FolderLoader loader = new(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 4, loader);

        winChance.Should().Be(0.557242);
        tieChance.Should().Be(0.005764);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_Returns343988_For_AKs_4Opponents()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 4, loader);

        winChance.Should().Be(0.343988);
        tieChance.Should().Be(0.019834);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_Returns3509_For_A4o_2Opponents()
    {
        Pair playerCards = new(new Card(4, CardSuit.Spades, true), new Card(14, CardSuit.Hearts, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 2, loader);


        winChance.Should().Be(0.3509);
        tieChance.Should().Be(0.04338);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_Returns661998_For_AKs_1Opponent()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 1, loader);


        winChance.Should().Be(0.661998);
        tieChance.Should().Be(0.0164);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_Throws_When_TooManyOpponents()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        Assert.Throws<KeyNotFoundException>(() => ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 10, loader));
    }


    [Fact]
    public void GetWinningChanceSim_Probabilities_InRange()
    {
        Deck deck = new();
        Pair holeCards = new(deck.NextCard(), deck.NextCard());
        List<Card> community = deck.NextCards(5);

        var (win, tie) = ChanceCalculator.GetWinningChanceSim(holeCards, community, 4, 1000);

        win.Should().BeInRange(0.0, 1.0);
        tie.Should().BeInRange(0.0, 1.0);
        (win + tie).Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void GetWinningChancePreFlopSim_Probabilities_InRange()
    {
        Deck deck = new();
        Pair holeCards = new Pair(deck.NextCard(), deck.NextCard());

        var (win, tie) = ChanceCalculator.GetWinningChancePreFlopSim(holeCards, 4, 1000);

        win.Should().BeInRange(0.0, 1.0);
        tie.Should().BeInRange(0.0, 1.0);
        (win + tie).Should().BeLessThanOrEqualTo(1.0);
    }


    [Fact]
    public void GetWinningChanceSim_Throws_When_DuplicateCards()
    {
        // Given
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(2, CardSuit.Spades, true));
        List<Card> communityCards = new()
        {
            new Card(4, CardSuit.Spades, false),
            new Card(5, CardSuit.Spades, false),
            new Card(6, CardSuit.Spades, false),
            new Card(7, CardSuit.Spades, false),
            new Card(8, CardSuit.Spades, false),
        };

        // Then
        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetWinningChanceSim(holeCards, communityCards, 4, 500));
    }

    [Fact]
    public void GetWinningChancePreFlopSim_Throws_When_DuplicateCards()
    {
        // Given
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(2, CardSuit.Spades, true));

        // Then
        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetWinningChancePreFlopSim(holeCards, 4, 500));
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_Throws_When_DuplicateCards()
    {
        // Given
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(2, CardSuit.Spades, true));
        FolderLoader folderLoader = new(pathToPreFlopDirectory);

        // Then
        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetWinningChancePreFlopLookUp(holeCards, 4, folderLoader));
    }

    [Fact]
    public void GetWinningChancePreFlopChen_Throws_When_DuplicateCards()
    {
        // Given
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(2, CardSuit.Spades, true));

        // Then
        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetWinningChancePreFlopChen(holeCards));
    }

    [Fact]
    public void GetPreFlopChen_Throws_When_DuplicateCards()
    {
        // Given
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(2, CardSuit.Spades, true));

        // Then
        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetPreFlopChen(holeCards));
    }


    [Fact]
    public void GetWinningChancePreFlopLookUp_AKo_KAo_Symmetric()
    {
        // Given
        Pair AKo = new(new Card(14, CardSuit.Diamonds, true), new Card(13, CardSuit.Hearts, true));
        Pair KAo = new(new Card(13, CardSuit.Diamonds, true), new Card(14, CardSuit.Hearts, true));
        FolderLoader loader = new(pathToPreFlopDirectory);
        // When
        (double winAKo, double tieAKo) = ChanceCalculator.GetWinningChancePreFlopLookUp(AKo, 4, loader);
        (double winKAo, double tieKAo) = ChanceCalculator.GetWinningChancePreFlopLookUp(KAo, 4, loader);
        // Then
        winAKo.Should().BeApproximately(winKAo, 0.01);
        tieAKo.Should().BeApproximately(tieKAo, 0.01);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_27o_72o_Symmetric()
    {
        // Given
        Pair o27 = new(new Card(2, CardSuit.Diamonds, true), new Card(7, CardSuit.Hearts, true));
        Pair o72 = new(new Card(7, CardSuit.Diamonds, true), new Card(2, CardSuit.Hearts, true));
        FolderLoader loader = new(pathToPreFlopDirectory);
        // When
        (double win27, double tie27) = ChanceCalculator.GetWinningChancePreFlopLookUp(o27, 4, loader);
        (double win72, double tie72) = ChanceCalculator.GetWinningChancePreFlopLookUp(o72, 4, loader);
        // Then
        win27.Should().BeApproximately(win72, 0.01);
        tie27.Should().BeApproximately(tie72, 0.01);
    }


    [Fact]
    // ! https://homes.luddy.indiana.edu/kapadia/nofoldem/5_wins.stats
    public void GetWinningChancePreFlopLookUp_ExternalValidation_9As()
    {
        // Given
        double external9AsWin = 0.2666;
        double external9AsTie = 0.0359;
        Pair pair = new(new Card(9, CardSuit.Spades, true), new Card(14, CardSuit.Spades, true));
        FolderLoader loader = new(pathToPreFlopDirectory);
        // When
        (double win, double tie) = ChanceCalculator.GetWinningChancePreFlopLookUp(pair, 4, loader);
        // Then
        win.Should().BeApproximately(external9AsWin, 0.01);
        tie.Should().BeApproximately(external9AsTie, 0.01);
    }

    [Fact]
    // ! https://homes.luddy.indiana.edu/kapadia/nofoldem/5_wins.stats
    public void GetWinningChancePreFlopLookUp_ExternalValidation_KKo()
    {
        // Given
        double externalKKoWin = 0.4953;
        double externalKKoTie = 0.0065;
        Pair pair = new(new Card(13, CardSuit.Spades, true), new Card(13, CardSuit.Diamonds, true));
        FolderLoader loader = new(pathToPreFlopDirectory);
        // When
        (double win, double tie) = ChanceCalculator.GetWinningChancePreFlopLookUp(pair, 4, loader);
        // Then
        win.Should().BeApproximately(externalKKoWin, 0.01);
        tie.Should().BeApproximately(externalKKoTie, 0.01);
    }

    [Fact]
    // ! https://homes.luddy.indiana.edu/kapadia/nofoldem/5_wins.stats
    public void GetWinningChancePreFlopLookUp_ExternalValidation_27o()
    {
        // Given
        double external27oWin = 0.0972;
        double external27oTie = 0.0287;
        Pair pair = new(new Card(2, CardSuit.Spades, true), new Card(7, CardSuit.Diamonds, true));
        FolderLoader loader = new(pathToPreFlopDirectory);
        // When
        (double win, double tie) = ChanceCalculator.GetWinningChancePreFlopLookUp(pair, 4, loader);
        // Then
        win.Should().BeApproximately(external27oWin, 0.01);
        tie.Should().BeApproximately(external27oTie, 0.01);
    }


    [Fact]
    public void GetWinningChanceSimParallel_and_GetWinningChanceSim_Symmetric()
    {
        // Given
        Pair holeCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Clubs, true));
        List<Card> community = new()
        {
            new Card(7, CardSuit.Hearts, false),
            new Card(8, CardSuit.Hearts, false),
            new Card(9, CardSuit.Hearts, false),
            new Card(12, CardSuit.Hearts, false),
            new Card(8, CardSuit.Diamonds, false),
        };

        // When
        (double simWin, double simTie) = ChanceCalculator.GetWinningChanceSim(holeCards, community, 4, 500_000);
        (double parallelWin, double parallelTie) = ChanceCalculator.GetWinningChanceSimParallel(holeCards, community, 4, 500_000);

        // Then
        simWin.Should().BeApproximately(parallelWin, 0.001);
        simTie.Should().BeApproximately(parallelTie, 0.001);
    }

    [Fact]
    public void GetWinningChancePreFlopSimParallel_and_GetWinningChancePreFlopSim_Symmetric()
    {
        // Given
        Pair holeCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Clubs, true));

        // When
        (double simWin, double simTie) = ChanceCalculator.GetWinningChancePreFlopSim(holeCards, 4, 500_000);
        (double parallelWin, double parallelTie) = ChanceCalculator.GetWinningChancePreFlopSimParallel(holeCards, 4, 500_000);

        // Then
        simWin.Should().BeApproximately(parallelWin, 0.001);
        simTie.Should().BeApproximately(parallelTie, 0.001);
    }
    
}