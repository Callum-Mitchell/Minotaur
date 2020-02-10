# Minotaur

A game created for the Winter 2018 Global Game Jam. You are trapped in a maze and must find the exit while avoiding the deadly Minotaur that roam its interior. But don't make too much noise; the minotaur will hunt down any disturbances they sense!

This fully playable game prototype was build using the Unity game engine and programmed in C#, with the exception of Unity's built-in first person controller. Mazes are procedurally generated on each play using a tile system and custom generation algorithm which guarantees all areas of the maze are accessible from the exit, and allows for tuning of aspects such as number of split paths and dead ends. The game makes use of spatial sound with which the player can sense the whereabouts of a Minotaur which they cannot see. Similarly, the Minotaur can sense the player's location and pursue them by the sounds they make; this effect was achieved through code. The Minotaur navigate the maze using the A* pathfinding algorithm.

Environments were build using free assets from the Unity Assets store. All assets, including models, textures, sounds, environments, and core programming were obtained, implemented and completed during a 48-hour period. A few following additions were made based on player feedback to improve game balance, with room and ideas for expansion into a full-scale, commercial-quality video game.
