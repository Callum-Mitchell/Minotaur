using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class describes a single tile within a generated maze. It includes information on the tile's location,
//connected paths, and a few other pieces of information that are used in maze generation/pathfinding
public class MazeTile {

    public MazeTile top = null;
    public MazeTile bottom = null;
    public MazeTile left = null;
    public MazeTile right = null;

    public int row = 0;
    public int column = 0;

    public bool isEntranceConnected = false; //Means at least 1 path exists from this tile to the maze entrance
    public bool isAdjacentToEntranceConnection = false; //Means one or more of the tiles next to it is entrance-connected
    public int entranceDistance = -1; //Distance between the current node and the entrance

    public enum direction
    {
        TOP, LEFT, BOTTOM, RIGHT
    };

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    //Creates a path between the current tile node and another adjacent node passed in
    //NOTE: During generation, only create connections from tiles which are entrance-connected, and only adjacent ones
    void createConnection(MazeTile otherTile, direction connectionDirection)
    {
        switch(connectionDirection)
        {
            case direction.TOP:
                top = otherTile;
                otherTile.bottom = this;
                break;
            case direction.BOTTOM:
                bottom = otherTile;
                otherTile.top = this;
                break;
            case direction.LEFT:
                left = otherTile;
                otherTile.right = this;
                break;
            case direction.RIGHT:
                right = otherTile;
                otherTile.left = this;
                break;

        }

        if (!otherTile.isEntranceConnected || otherTile.entranceDistance > entranceDistance)
        {
            otherTile.entranceDistance = entranceDistance + 1;
        }
        otherTile.isEntranceConnected = true;
    }

}
