# PokerAlgo
#### An algorithm to determine the winner(s) of a Texas Hold 'em game. Might not be the most efficient one out there, but it's mine.
---
- The core logic to a Texas Hold 'em game I'm working on.
- Would like to release as nuget package so anyone can use it, and so I can easily reuse it in other projects.
## Features
- Determines the "winning hand" of a given Player.  
- Determines the winner or winners in case of a tie.
- Given a winning hand, it will provide a "pretty" name. For example, "Full House, 7s over 5s" or "Pair of 7s".
- Provides the chances of winning or tying a round of Texas Hold 'em.
  - Given a Player's hole cards, it can provide a hand strength value using Bill Chen's formula. A value ranging from -1 to 22.
  - Pre-flop, given a Player's hole cards, it can provide the chances of winning or tying using Monte Carlo simulations or using pre-computed data from a file.
  - Post-flop, it can compute the chances of a player winning or tying using Monte Carlo simulations.
  > Something to note about these predictions is that they are independent of other players. They are only calculated based on what a given player knows, hence why the chances don't add to 100%. The reason I added predictions to begin with is so I could build an AI that would make decisions based on its probabilities of winning.
## Quick Log and Ideas
- 4/6/2025: Implemented custom exceptions. Added better input validation and added an internal Guards class that handles said input validation. Because of this and further testing I discovered another little logic bug in ChanceCalculator.GetWinningChancePreFlopSim() in which the deck was used before removing known cards meaning a chance of having duplicate cards. Added a new project under the same sln file called PokerAlgo.Compute. Its sole purpose is to compute preflop data and output it as files that can be used by the PokerAlgo. GetWinningChanceSim now simulates remaining community cards, back then it made sense since I wanted to know the "current" chances of winning but that makes no sense because of the probability of opponents getting a better hand. Add parallelization for Monte Carlo simulations which made them ~4.39x faster at least on my PC. I renamed all unit tests for consistency and readability. It's late and I would like to review the parallelized Monte Carlo code tomorrow to make sure there are no logical weirdness. Apart from that this part of the project is almost done. I might not upload it as a nuget package just yet, but I will prepare it.  
The next step for the game would be a prototype with the full game logic and a Rule-Based AI. The most challenging part of the the game logic will probably be pot splitting. As for the AI I'm thinking of using a mathematical model that takes in chances of winning, the chips the AI has bet vs how much it has left, and the opponents' "confidence" to make a choice on whether to call/check, raise, or fold. I'm thinking of using Raylib-cs to visualize the state of the game.

- 3/27/2025: I have restructured the project after learning more about how bigger .NET projects are structured. The main project is PokerAlgo which contains the core code. PokerAlgo.Sandbox is the CLI utility that allows me to manually test the library. PokerAlgo.Testing is an xunit project for proper unit testing. After doing so I started using a more TDD approach. Added a shared "Resources" directory that holds data such as the Pre-Computed Pre-Flop data.  
After I have designed and implemented the functionality to load the pre-flop chances data from files, I would like to perform a code review to see if there are any features I would like to add and implement custom exceptions for more concise and standardized error handling. I would like to multithread the Monte Carlo simulations to improve performance. The last thing would be turning this project into a nuget package.

- 3/3/2025: I have added better loggging, refactored a few major parts of the codebase, changed the file structure for packaging, made new tests for the Algo and HandEvaluator classes, implemented Bill Chen's formula for calculating the strength of pre-flop then used this value and created a mathematical function that uses Chen's value to give a rough approximation of the chances of winning. Implemented Monte Carlo simulations for pre-flop hands (computed values match expected results) as well as a way of computing and outputting the results to a file, I might use this to make a lookup table for preflop hands to optimize pre-flop hand strength calculations. Improved error handling. Added the conditional attribute to Debug Log Functions in the Helpers class to prevent them from being compiled in release. I still need to implement the pre-flop hand lookup table using my own generated data and custom exceptions.

- 1/22/2025: Because of how I had structured the algorithm since the beginning I can see a way of simplifying winner determination. In the beginning of this project I had the wrong idea about how the winner was Determined in Texas Hold 'em. After learning how it worked a little bit into the project I never stepped back and redesigned the algorithm. Should be a smooth ride from here, it feels like I have everything figured out I just need to refactor the code. I also added a Monte Carlo Simulation to predict how likely a player is to win. I've been thinking about using an external dataset instead of creating my own tests. Making tests for this by hand is very tedious.

- 1/16/2025: Finished the algorithm. I can think of a few optimizations, I need to clean up debug logs. Need to look into testing frameworks or make my own, as well as test the code extensively. I would like to write custom exception classes, and document everything to package the project with nuget. I'm also thinking about including a simple probability model to predict how likely a player is to win (independent from other players); this number would allow me to make an enemy AI.

- 12/28/2024: Added better debug logging as well as verbosity levels. Optimized the first part quite a bit but could still improve further. Players now only have 1 winning hand, and now the best 5 cards get stored including the winning hand. This is so kickers are easier to compare. Added method to determine the community's winning hand. Need to rewrite how the winner is determined, it should be easier now since we get the best 5 cards.

- 12/4/2024: First half is done. It detects the type of winning hands a player has. I need to optimize this now. Then I would need to see what winning hands are on the community cards. Then the last part is to compare the hands of each player as well as the community.

## Output Example:
This is an example of the current output of the program that showcases its functionality. 

Running the program. 

![image](https://github.com/user-attachments/assets/6556408a-5260-4d1f-b8e5-ada240a295cf)

Determine Winning Hand for each player and determine winner(s). 

![image](https://github.com/user-attachments/assets/c9a6b185-8a5f-45ee-8e0e-84173537eeaa)

Determine chances of a players winning. 

![image](https://github.com/user-attachments/assets/6d153dc1-48df-4887-94bc-948c1a67b287)

Determine pre-flop chances of winning (Chen's + Custom Sigmoid Equation). 

![image](https://github.com/user-attachments/assets/dfc50d13-2f92-41ab-9468-4133066bd520)

Compute pre-flop chances of winning (Monte Carlo) and output to a file. 

![image](https://github.com/user-attachments/assets/181576db-a0a5-4d7c-9a6b-449d24842dcd)

Output file. 

![image](https://github.com/user-attachments/assets/204f85d1-328e-41d2-beea-61167dd8ccaf)
