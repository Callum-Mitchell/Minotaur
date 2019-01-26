using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeLayout : MonoBehaviour {

    public List< List<MazeTile> > tiles; //Stores all tiles in the maze
    public List<MazeTile> tilesAboutToBeConnected; //Stores all tiles not connected to entrance, but adjacent to one that is
    public int mazeWidth;
    public int mazeDepth;
    public int totalTiles;
    public int accessibleTiles;
    public Vector2 tileSize; //In global Unity transform units

    public Vector2 entranceTilePosition; //In global Unity transform units

    // Use this for initialization
    void Start () {
        totalTiles = mazeWidth * mazeDepth;

        //Set up correctly-sized 2D tile list
        for (int i = 0; i < mazeWidth; i++)
        {
            List<MazeTile> tileColumn = new List<MazeTile>();
            for (int j = 0; j < mazeDepth; j++)
            {
                MazeTile tile = new MazeTile();
                tile.row = j;
                tile.column = i;
                tileColumn.Add(tile);
            }
            tiles.Add(tileColumn);
        }
        accessibleTiles = 1;

        tiles[0][0].isEntranceConnected = true;
        tilesAboutToBeConnected.Add(tiles[0][1]);
        tilesAboutToBeConnected.Add(tiles[1][0]);
    }

    // Update is called once per frame
    void Update () {
		
	}

    //Generates a maze layout such that every tile can be reached from the entrance
    void generateLayout()
    {
        //Process:
        //Generate random integer between 0 and (size of tilesAboutToBeConnected)
        //Access this tile in tiles[][].
        //If this tile is connected to the entrance, generate another random int between 0 and 3.
        //0 = top, 1 = right, 2 = bottom, 3 = left
        //Check if the current node's "top" is entrance-connected. If so, connect the 2 nodes.
        //Else, check right. Else, bottom, else, left (one of them should be connected or something has gone wrong.
        //After connection, check each adjacent node for entrance-connection and e-connection adjacency.
        //For each one that is NOT entrance-connected and whose isAdjacentToEntranceConnection is false,
        //set true and add to adjancent connenctions list
        //Remove current node from adjacent connections list.
        //Repeat until adjacent connections list is empty (meaning all tiles are accessible!) or until
        //total entrance-connected tile count == total tile count. Either condition works.


        //Once all tiles can be accessed from the entrance, go through each tile starting from the [0][0] (bottom-left) and go in raster order.
        //For each one, check "isTopConnected" and add horizontal wall if false, then check "isRightConnected"
        //and add vertical wall if false.

    }
}
