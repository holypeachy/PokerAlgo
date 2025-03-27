namespace PokerAlgo.Testing;

public class ChanceCalculatorTests
{
    [Fact]
    public void GetWinningChanceSim_no_less_than_3_cummunity_cards()
    {
        Deck deck = new();
        Pair<Card, Card> playerCards = new Pair<Card, Card>(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
        };

        Assert.Throws<ArgumentException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 2, 500));
    }

    [Fact]
    public void GetWinningChanceSim_no_more_than_5_cummunity_cards()
    {
        Deck deck = new();
        Pair<Card, Card> playerCards = new Pair<Card, Card>(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        Assert.Throws<ArgumentException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 2, 500));
    }

    [Fact]
    public void GetWinningChanceSim_at_least_1_opponent_to_simulate()
    {
        Deck deck = new();
        Pair<Card, Card> playerCards = new Pair<Card, Card>(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        Assert.Throws<ArgumentException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 0, 500));
    }

    [Fact]
    public void GetWinningChanceSim_at_least_100_sim_games_for_chance_prediction()
    {
        Deck deck = new();
        Pair<Card, Card> playerCards = new Pair<Card, Card>(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        Assert.Throws<ArgumentException>(() => ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 4, 10));
    }

    [Fact]
    public void GetWinningChanceSim_no_errors()
    {
        Deck deck = new();
        Pair<Card, Card> playerCards = new Pair<Card, Card>(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        ChanceCalculator.GetWinningChanceSim(playerCards, communityCards, 4, 500);
    }



    [Fact]
    public void GetWinningChancePreFlopSim_at_least_1_opponent_to_simulate()
    {
        Deck deck = new();
        Pair<Card, Card> playerCards = new Pair<Card, Card>(deck.NextCard(), deck.NextCard());
        List<Card> communityCards = new()
        {
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
            deck.NextCard(),
        };

        Assert.Throws<ArgumentException>(() => ChanceCalculator.GetWinningChancePreFlopSim(playerCards, 0, 500));
    }

    [Fact]
    public void GetWinningChancePreFlopSim_at_least_100_sim_games_for_chance_prediction()
    {
        // Given

        // When

        // Then
    }


    [Fact]
    public void GetPreFlopChen_Chen_value_for_AK_suited_is_12()
    {
        Pair<Card, Card> holeCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(12);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_TT_offsuit_is_10()
    {
        Pair<Card, Card> holeCards = new(new Card(10, CardSuit.Spades, true), new Card(10, CardSuit.Hearts, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(10);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_57_suited_is_6()
    {
        Pair<Card, Card> holeCards = new(new Card(5, CardSuit.Spades, true), new Card(7, CardSuit.Spades, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(6);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_27_offsuit_is_neg_1()
    {
        Pair<Card, Card> holeCards = new(new Card(2, CardSuit.Spades, true), new Card(7, CardSuit.Hearts, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(-1);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_AA_offsuit_is_20()
    {
        Pair<Card, Card> holeCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Hearts, true));

        ChanceCalculator.GetPreFlopChen(holeCards).Should().Be(20);
    }

    [Fact]
    public void GetPreFlopChen_Chen_value_for_AA_suited_is_22_but_should_throw_error_because_not_possible_in_game()
    {
        Pair<Card, Card> holeCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Spades, true));

        Assert.Throws<ArgumentException>(() => ChanceCalculator.GetPreFlopChen(holeCards));
    }


    [Fact]
    public void GetWinningChancePreFlopLookUp_AAo_4_opponents_61413()
    {
        Pair<Card, Card> playerCards = new(new Card(14, CardSuit.Spades, true), new Card(14, CardSuit.Clubs, true));
        string path = @"C:/Users/Frank/Code/PokerAlgo/Resources/Preflop_data/4_200k.preflop";

        ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 4, path).Item1.Should().Be(0.61413);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_AKs_4_opponents_41133()
    {
        Pair<Card, Card> playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        string path = @"C:/Users/Frank/Code/PokerAlgo/Resources/Preflop_data/4_200k.preflop";

        ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 4, path).Item1.Should().Be(0.41133);
    }

    [Fact]
    public void GetWinningChancePreFlopLookUp_AKs_1_opponent_707155_directory()
    {
        Pair<Card, Card> playerCards = new(new Card(14, CardSuit.Spades, true), new Card(13, CardSuit.Spades, true));
        string directoryPath = @"C:/Users/Frank/Code/PokerAlgo/Resources/Preflop_data/";

        ChanceCalculator.GetWinningChancePreFlopLookUp(playerCards, 1, directoryPath).Item1.Should().Be(0.707155);
    }

}