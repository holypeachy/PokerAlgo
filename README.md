# poker-algo
#### An algorithm to determine the winner(s) of a Texas Hold 'em game. Might not be the most efficient one out there, but it's mine. It runs in under 40ms.

---
- The core logic to a Texas Hold 'em game I'm working on.
- Would like to release as nuget package so anyone can use it, and so I can easily reuse it in other projects.
## Quick Log and Ideas
- 12/4/2024: First half is done. It detects the type of winning hands a player has. I need to optimize this now. Then I would need to see what winning hands are on the community cards. Then the last part is to compare the hands of each player as well as the community.

- 12/28/2024: Added better debug logging as well as verbosity levels. Optimized the first part quite a bit but could still improve further. Players now only have 1 winning hand, and now the best 5 cards get stored including the winning hand. This is so kickers are easier to compare. Added method to determine the community's winning hand. Need to rewrite how the winner is determined, it should be easier now since we get the best 5 cards.

- 1/16/2025: Finished the algorithm. I can think of a few optimizations, I need to clean up debug logs. Need to look into testing frameworks or make my own, as well as test the code extensively. I would like to write custom exception classes, and document everything to package the project with nuget. I'm also thinking about including a simple probability model to predict how likely a player is to win (independent from other players); this number would allow me to make an enemy AI.

## Output Example:
This is an example of the current output of the program that showcases its functionality.

![image](https://github.com/user-attachments/assets/24fa8de4-3f50-478b-95bc-74daebec9657)
![image](https://github.com/user-attachments/assets/12d0f529-36f6-465a-a783-6224a4608a64)
![image](https://github.com/user-attachments/assets/61567bc2-11a4-49cb-82c3-3a6cb3164e55)
