Started work on my Texas Hold 'em game and the most important logic is the algorithm that determines whose hand wins.

12/4/2024: First half is done. It detects the type of winning hands a player has. I need to optimize this now. Then I would need to see what winning hands are on the community cards. Then the last part is to compare the hands of each player as well as the community.

12/28/2024: Added better debug logging as well as verbosity levels. Optimized the first part quite a bit but could still improve further. Players now only have 1 winning hand, and now the best 5 cards get stored including the winning "combo." Added method to determine the community's winning hand. Need to rewrite how the winner is determined, it should be easier now since we get the best 5 cards.
