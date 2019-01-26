using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeLayout : MonoBehaviour {

    public GameObject horizontalWall;
    public GameObject verticalWall;
    public GameObject Pillar;

    public List< List<MazeTile> > tiles; //Stores all tiles in the maze. 1st index is column, 2nd is row
    public List<MazeTile> boundaryTiles; //Stores all entrance-connected tiles that are adjacent to non entrance-connected ones
    public int mazeWidth;
    public int mazeDepth;
    public int totalTiles;
    public int accessibleTiles;
    public float tileWidth; //In global Unity transform units
    public float tileDepth;

    public float entranceTileXPosition; //In global Unity transform units
    public float entranceTileZPosition; //In global Unity transform units

    //0 to 1 - sets the probablility that, after a connection from one maze tile to an adjacent one,
    //the generation algorithm will keep checking for another 
    //Higher values will lead to more branching paths. Lower values will lead to more linearity.
    public float additionalConnectionThreshold = 0.0f;

    //0 to 1 - sets the probability that, if a potential connection would connect two tiles that are both
    //already entrance-accessible, the connection will be created anyway.
    //Higher values will lead to more loops and branching. Lower values will lead to linearity and segregation.
    public float redundantConnectionThreshold = 0.0f;

    // Use this for initialization
    void Start () {
        tiles = new List<List<MazeTile>>();
        boundaryTiles = new List<MazeTile>();
        totalTiles = mazeWidth * mazeDepth;
        accessibleTiles = 1;

        //Set up correctly-sized 2D tile list
        for (int i = 0; i < mazeWidth; i++)
        {
            tiles.Add(new List<MazeTile>());
            for (int j = 0; j < mazeDepth; j++)
            {
                MazeTile tile = new MazeTile();
                tile.row = j;
                tile.column = i;
                tiles[i].Add(tile);
            }
        }
        accessibleTiles = 1;

        tiles[0][0].isEntranceConnected = true;
        boundaryTiles.Add(tiles[0][0]);

        generateLayout();
    }

    // Update is called once per frame
    void Update () {
		
	}

    //Generates a maze layout such that every tile can be reached from the entrance
    void generateLayout()
    {
        while (accessibleTiles < mazeDepth * mazeWidth)
        {
            //Process:
            //Generate random integer between 0 and (size of tilesAboutToBeConnected)
            int adjacentTileIndex = Random.Range(0, boundaryTiles.Count);
            //Access this tile in tiles[][].
            MazeTile tileToConnect = boundaryTiles[adjacentTileIndex];
            tileToConnect = tiles[tileToConnect.column][tileToConnect.row];
            //Now generate another random int between 0 and 3.
            int connectionDirection = Random.Range(0, 3);
            //0 = top, 1 = right, 2 = bottom, 3 = left
            //Eg. If starting at top, check if the current node's "top" is entrance-connected. If not, connect the 2 nodes.
            //If already entrance-connected, generate random 0-1 number, and if it's less than a certain threshold, connect anyway.
            //Else, check right. Else, bottom, else, left (one of them should be connected or something has gone wrong).
            int firstConnectionDirection = connectionDirection;
            do
            {
                MazeTile otherTile = new MazeTile();
                if (connectionDirection == (int)MazeConnectionDirection.TOP && tileToConnect.row < mazeDepth - 1)
                {
                    //Top
                    otherTile = tiles[tileToConnect.column][tileToConnect.row + 1];
                    if (!otherTile.isEntranceConnected || Random.Range(0.0f, 1.0f) < redundantConnectionThreshold)
                    {
                        if (!otherTile.isEntranceConnected)
                        {
                            accessibleTiles++;
                            //Check if new connected tile is at edge of maze. If so, add to that list
                            if ((otherTile.row < mazeDepth - 1 && !tiles[otherTile.column][otherTile.row + 1].isEntranceConnected)
                                || (otherTile.row > 0 && !tiles[otherTile.column][otherTile.row - 1].isEntranceConnected)
                                || (otherTile.column < mazeWidth - 1 && !tiles[otherTile.column + 1][otherTile.row].isEntranceConnected)
                                || (otherTile.column > 0 && !tiles[otherTile.column - 1][otherTile.row].isEntranceConnected))
                            {
                                boundaryTiles.Add(otherTile);
                            }
                        }
                        tileToConnect.createConnection(otherTile, (MazeConnectionDirection)connectionDirection);
                    }
                }
                else if (connectionDirection == (int)MazeConnectionDirection.BOTTOM && tileToConnect.row > 0)
                {
                    //Bottom
                    otherTile = tiles[tileToConnect.column][tileToConnect.row - 1];
                    if (!otherTile.isEntranceConnected || Random.Range(0.0f, 1.0f) < redundantConnectionThreshold)
                    {
                        if (!otherTile.isEntranceConnected) accessibleTiles++;
                        {
                            //Check if new connected tile is at edge of maze. If so, add to that list
                            if ((otherTile.row < mazeDepth - 1 && !tiles[otherTile.column][otherTile.row + 1].isEntranceConnected)
                                || (otherTile.row > 0 && !tiles[otherTile.column][otherTile.row - 1].isEntranceConnected)
                                || (otherTile.column < mazeWidth - 1 && !tiles[otherTile.column + 1][otherTile.row].isEntranceConnected)
                                || (otherTile.column > 0 && !tiles[otherTile.column - 1][otherTile.row].isEntranceConnected))
                            {
                                boundaryTiles.Add(otherTile);
                            }
                        }
                        tileToConnect.createConnection(otherTile, (MazeConnectionDirection)connectionDirection);
                    }
                }
                else if (connectionDirection == (int)MazeConnectionDirection.LEFT && tileToConnect.column > 0)
                {
                    //Left
                    otherTile = tiles[tileToConnect.column - 1][tileToConnect.row];
                    if (!otherTile.isEntranceConnected || Random.Range(0.0f, 1.0f) < redundantConnectionThreshold)
                    {
                        if (!otherTile.isEntranceConnected)
                        {
                            accessibleTiles++;
                            //Check if new connected tile is at edge of maze. If so, add to that list
                            if ((otherTile.row < mazeDepth - 1 && !tiles[otherTile.column][otherTile.row + 1].isEntranceConnected)
                                || (otherTile.row > 0 && !tiles[otherTile.column][otherTile.row - 1].isEntranceConnected)
                                || (otherTile.column < mazeWidth - 1 && !tiles[otherTile.column + 1][otherTile.row].isEntranceConnected)
                                || (otherTile.column > 0 && !tiles[otherTile.column - 1][otherTile.row].isEntranceConnected))
                            {
                                boundaryTiles.Add(otherTile);
                            }
                        }
                        tileToConnect.createConnection(otherTile, (MazeConnectionDirection)connectionDirection);
                    }
                }
                else if (connectionDirection == (int)MazeConnectionDirection.RIGHT && tileToConnect.column < mazeWidth - 1)
                {
                    //Right
                    otherTile = tiles[tileToConnect.column + 1][tileToConnect.row];
                    if (!otherTile.isEntranceConnected || Random.Range(0.0f, 1.0f) < redundantConnectionThreshold)
                    {
                        if (!otherTile.isEntranceConnected)
                        {
                            accessibleTiles++;
                            //Check if new connected tile is at edge of maze. If so, add to that list
                            if ((otherTile.row < mazeDepth - 1 && !tiles[otherTile.column][otherTile.row + 1].isEntranceConnected)
                                || (otherTile.row > 0 && !tiles[otherTile.column][otherTile.row - 1].isEntranceConnected)
                                || (otherTile.column < mazeWidth - 1 && !tiles[otherTile.column + 1][otherTile.row].isEntranceConnected)
                                || (otherTile.column > 0 && !tiles[otherTile.column - 1][otherTile.row].isEntranceConnected))
                            {
                                boundaryTiles.Add(otherTile);
                            }
                        }
                        tileToConnect.createConnection(otherTile, (MazeConnectionDirection)connectionDirection);
                    }
                }

                connectionDirection++;
                if (connectionDirection > 3) connectionDirection = 0;

                //After each connection, check if the tile on each side of the newly-connected tile is still a boundary tile. If not, remove from list.
                for (int i = 0; i < 3; i++)
                {
                    MazeTile adjacentToConnectedTile;
                    if (i == (int)MazeConnectionDirection.TOP && otherTile.row < mazeDepth - 1)
                    {
                        adjacentToConnectedTile = tiles[otherTile.column][otherTile.row + 1];
                    }
                    else if (i == (int)MazeConnectionDirection.BOTTOM && otherTile.row > 0)
                    {
                        adjacentToConnectedTile = tiles[otherTile.column][otherTile.row - 1];
                    }
                    else if (i == (int)MazeConnectionDirection.LEFT && otherTile.column > 0)
                    {
                        adjacentToConnectedTile = tiles[otherTile.column - 1][otherTile.row];
                    }
                    else if (i == (int)MazeConnectionDirection.RIGHT && otherTile.column < mazeWidth - 1)
                    {
                        adjacentToConnectedTile = tiles[otherTile.column + 1][otherTile.row];
                    }

                    if ((tileToConnect.row < mazeDepth - 1 && !tiles[tileToConnect.column][tileToConnect.row + 1].isEntranceConnected)
                        || (tileToConnect.row > 0 && !tiles[tileToConnect.column][tileToConnect.row - 1].isEntranceConnected)
                        || (tileToConnect.column < mazeWidth - 1 && !tiles[tileToConnect.column + 1][tileToConnect.row].isEntranceConnected)
                        || (tileToConnect.column > 0 && !tiles[tileToConnect.column - 1][tileToConnect.row].isEntranceConnected))
                    {
                        //still a boundary tile
                    }
                    else
                    {
                        boundaryTiles.Remove(tileToConnect);
                    }
                }
                if (Random.Range(0, 1) > additionalConnectionThreshold) break;
            } while (firstConnectionDirection != connectionDirection);

            //Repeat until adjacent connections list is empty (meaning all tiles are accessible!) or until
            //total entrance-connected tile count == total tile count. Either condition works.
        }

        //Once all tiles can be accessed from the entrance, go through each tile starting from the [0][0] (bottom-left) and go in raster order.
        for(int col = 0; col < mazeWidth; col++)
        {
            for(int row = 0; row < mazeDepth; row++)
            {
                //For each one, check "isTopConnected" and add horizontal wall if false, then check "isRightConnected"
                //and add vertical wall if false.
                MazeTile currentTile = tiles[col][row];
                if(currentTile.top == null && row < mazeDepth - 1)
                {
                    Instantiate(horizontalWall, new Vector3(entranceTileXPosition + (tileDepth * (row + 0.5f)), 2.5f, entranceTileZPosition - (tileDepth * col)), Quaternion.identity);
                }
                if(currentTile.right == null && col < mazeWidth - 1)
                {
                    Instantiate(verticalWall, new Vector3(entranceTileXPosition + (tileDepth * row), 2.5f, entranceTileZPosition - (tileDepth * (col + 0.5f))), Quaternion.identity);
                }

                if (row < mazeDepth - 1 && col < mazeWidth - 1)
                {
                    //Also place stone pillar to fill gaps between dividers
                    Instantiate(Pillar, new Vector3(entranceTileXPosition + (tileDepth * (row + 0.5f)), 2.5f, entranceTileZPosition - (tileDepth * (col + 0.5f))), Quaternion.identity);
                }
            }
        }

    }
}
