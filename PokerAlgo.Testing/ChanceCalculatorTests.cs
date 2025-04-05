namespace PokerAlgo.Testing;

public class ChanceCalculatorTests
{
    public string pathToPreFlopDirectory = @"C:/Users/Frank/Code/PokerAlgo/Resources/preflop_data/";

    [Fact]
    public void GetWinningChanceSim_no_less_than_3_cummunity_cards()
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
    public void GetWinningChanceSim_no_more_than_5_cummunity_cards()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(6);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 2, 500));
    }

    [Fact]
    public void GetWinningChanceSim_at_least_1_opponent_to_simulate()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 0, 500));
    }

    [Fact]
    public void GetWinningChanceSim_at_least_100_sim_games_for_chance_prediction()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 4, 10));
    }

    [Fact]
    public void GetWinningChanceSim_no_errors()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 4, 500);
    }



    [Fact]
    public void GetWinningChancePreFlopSim_at_least_1_opponent_to_simulate()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChancePreFlopSim(playerCards, 0, 500));
    }

    [Fact]
    public void GetWinningChancePreFlopSim_at_least_100_sim_games_for_chance_prediction()
    {
        Deck deck = new();
        Pair playerCards = new Pair(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = deck.NextCards(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => ChanceCalculator.GetWinningChancePreFlopSim(playerCards, 0, 99));
    }


    [Fact]
    public void GetPreFlopChen_Chen_value_for_AK_suited_is_12()
    {
        Pair holeCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(12);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_TT_offsuit_is_10()
    {
        Pair holeCards = new(new Card(10, CardSuit.Spades, true), new Card(10, CardSuit.Hearts, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(10);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_57_suited_is_6()
    {
        Pair holeCards = new(new Card(5, CardSuit.Spades, true), new Card(7, CardSuit.Spades, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(6);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_27_offsuit_is_neg_1()
    {
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(7, CardSuit.Hearts, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(-1);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_AA_offsuit_is_20()
    {
        Pair holeCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Hearts, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(20);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_AA_suited_is_22_but_should_throw_error_because_not_possible_in_game()
    {
        Pair holeCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Spades, true));

        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetPreFlopChen(holeCards));
    }


    [Fact]
    public void GetWinningChancePreFlopLookUp_AAo_4_opponents_win_should_be_557242()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Clubs, true));
        FolderLoader loader = new(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 4, loader);

        winChance.Should().Be(0.557242);
        tieChance.Should().Be(0.005764);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_AKs_4_opponents_opponents_win_should_be_343988()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 4, loader);

        winChance.Should().Be(0.343988);
        tieChance.Should().Be(0.019834);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_4Ao_2_opponents_opponents_win_should_be_3509()
    {
        Pair playerCards = new(new Card(4, CardSuit.Spades, true), new Card(14, CardSuit.Hearts, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 2, loader);


        winChance.Should().Be(0.3509);
        tieChance.Should().Be(0.04338);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_AKs_1_opponent_opponents_win_should_be_661998()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 1, loader);


        winChance.Should().Be(0.661998);
        tieChance.Should().Be(0.0164);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_AKs_10_opponents_throws_KeyNotFoundException()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        Assert.Throws<KeyNotFoundException>(() => ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 10, loader));
    }


    [Fact]
    public void GetWinningChanceSim_Probabilities_Are_In_Valid_Range()
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
    public void GetWinningChancePreFlopSim_Probabilities_Are_In_Valid_Range()
    {
        Deck deck = new();
        Pair holeCards = new Pair(deck.NextCard(), deck.NextCard());

        var (win, tie) = ChanceCalculator.GetWinningChancePreFlopSim(holeCards, 4, 1000);

        win.Should().BeInRange(0.0, 1.0);
        tie.Should().BeInRange(0.0, 1.0);
        (win + tie).Should().BeLessThanOrEqualTo(1.0);
    }


    [Fact]
    public void GetWinningChanceSim_no_duplicate_cards()
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
    public void GetWinningChancePreFlopSim_no_duplicate_cards()
    {
        // Given
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(2, CardSuit.Spades, true));

        // Then
        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetWinningChancePreFlopSim(holeCards, 4, 500));
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_no_duplicate_cards()
    {
        // Given
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(2, CardSuit.Spades, true));
        FolderLoader folderLoader = new(pathToPreFlopDirectory);

        // Then
        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetWinningChancePreFlopLookUp(holeCards, 4, folderLoader));
    }

    [Fact]
    public void GetWinningChancePreFlopChen_no_duplicate_cards()
    {
        // Given
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(2, CardSuit.Spades, true));

        // Then
        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetWinningChancePreFlopChen(holeCards));
    }

    [Fact]
    public void GetPreFlopChen_no_duplicate_cards()
    {
        // Given
        Pair holeCards = new(new Card(2, CardSuit.Spades, true), new Card(2, CardSuit.Spades, true));

        // Then
        Assert.Throws<DuplicateCardException>(() => ChanceCalculator.GetPreFlopChen(holeCards));
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_symmetry_AKo_and_KAo_have_similar_values()
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
    public void GetWinningChancePreFlopLookUp_symmetry_27o_and_72o_have_similar_values()
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
    public void GetWinningChancePreFlopLookUp_external_dataset_validation_9As()
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
    public void GetWinningChancePreFlopLookUp_external_dataset_validation_KKo()
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
    public void GetWinningChancePreFlopLookUp_external_dataset_validation_27o()
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
    
}