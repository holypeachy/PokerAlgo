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
    public void GetWinningChancePreFlopLookUp_AAo_4_opponents_win_should_be_61413()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Clubs, true));
        FolderLoader loader = new(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 4, loader);

        winChance.Should().Be(0.61413);
        tieChance.Should().Be(0.00555);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_AKs_4_opponents_opponents_win_should_be_41133()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 4, loader);

        winChance.Should().Be(0.41133);
        tieChance.Should().Be(0.022495);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_4Ao_2_opponents_opponents_win_should_be_400275()
    {
        Pair playerCards = new(new Card(4, CardSuit.Spades, true), new Card(14, CardSuit.Hearts, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 2, loader);


        winChance.Should().Be(0.400275);
        tieChance.Should().Be(0.043665);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_AKs_1_opponent_opponents_win_should_be_707155()
    {
        Pair playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        IPreFlopDataLoader loader = new FolderLoader(pathToPreFlopDirectory);

        (double winChance, double tieChance) = ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 1, loader);


        winChance.Should().Be(0.707155);
        tieChance.Should().Be(0.015345);
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
    
}