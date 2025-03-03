# PokerAlgo
#### An algorithm to determine the winner(s) of a Texas Hold 'em game. Might not be the most efficient one out there, but it's mine.

---
- The core logic to a Texas Hold 'em game I'm working on.
- Would like to release as nuget package so anyone can use it, and so I can easily reuse it in other projects.
## Quick Log and Ideas
- 3/3/2025: I have added better loggging, refactored a few major parts of the codebase, changed the file structure for packaging, made new tests for the Algo and HandEvaluator classes, implemented Bill Chen's formula for calculating the strength of pre-flop then used this value and created a mathematical function that uses Chen's value to give a rough approximation of the chances of winning. Implemented Monte Carlo simulations for pre-flop hands (computed values match expected results) as well as a way of computing and outputting the results to a file, I might use this to make a lookup table for preflop hands to optimize pre-flop hand strength. Improved error handling. Added the conditional attribute to Debug Log Functions in the Helpers class to prevent them from being compiled in release. I still need to implement the pre-flop hand lookup table using my own generated data and custom exceptions.
 
- 1/22/2025: Because of how I had structured the algorithm since the beginning I can see a way of simplifying winner determination. In the beginning of this project I had the wrong idea about how the winner was Determined in Texas Hold 'em. After learning how it worked a little bit into the project I never stepped back and redesigned the algorithm. Should be a smooth ride from here, it feels like I have everything figured out I just need to refactor the code. I also added a Monte Carlo Simulation to predict how likely a player is to win. I've been thinking about using an external dataset instead of creating my own tests. Making tests for this by hand is very tedious.

- 1/16/2025: Finished the algorithm. I can think of a few optimizations, I need to clean up debug logs. Need to look into testing frameworks or make my own, as well as test the code extensively. I would like to write custom exception classes, and document everything to package the project with nuget. I'm also thinking about including a simple probability model to predict how likely a player is to win (independent from other players); this number would allow me to make an enemy AI.

- 12/28/2024: Added better debug logging as well as verbosity levels. Optimized the first part quite a bit but could still improve further. Players now only have 1 winning hand, and now the best 5 cards get stored including the winning hand. This is so kickers are easier to compare. Added method to determine the community's winning hand. Need to rewrite how the winner is determined, it should be easier now since we get the best 5 cards.

- 12/4/2024: First half is done. It detects the type of winning hands a player has. I need to optimize this now. Then I would need to see what winning hands are on the community cards. Then the last part is to compare the hands of each player as well as the community.

## Output Example:
This is an example of the current output of the program that showcases its functionality. 

Running the program. 

![image](https://github.com/user-attachments/assets/200029eb-2308-4294-8a27-5fc63cc9811f)

Determine Winning Hand for each player and determine winner(s). 

![image](https://github.com/user-attachments/assets/003148af-ad38-4226-a662-4883522397ce)

Determine chances of a players winning. 

![image](https://github.com/user-attachments/assets/71f27aa0-46d0-4479-a658-fb89a1016015)

Determine pre-flop chances of winning (Chen's + Custom Sigmoid Equation). 

![image](https://github.com/user-attachments/assets/4e0983a0-8f57-471c-ad01-963ffda8b5ca)

Compute pre-flop chances of winning (Monte Carlo) and output to a file. 

![image](https://github.com/user-attachments/assets/c6192874-f9d5-4d9a-9238-f5e7f29af4e9)

Output in data.txt. 

![image](https://github.com/user-attachments/assets/9bca59ba-b4c8-43f3-bf86-e538222286ae)
