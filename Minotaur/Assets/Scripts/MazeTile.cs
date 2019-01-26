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

    public bool isEntranceConnected = false; //Means at least 1 path exists from this tile to the maze entrance
    public int entranceDistance = -1;

    public enum direction
    {
        TOP = 0, BOTTOM = 1, LEFT = 2, RIGHT = 3 
    };

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    //Creates a path between two MazeTile nodes. NOTE: During generation, only create connections from tiles which are entrance-connected
    void createConnection(MazeTile otherTile, direction connectionDirection)
    {
        switch(connectionDirection)
        {
            case direction.TOP:
                top = new MazeTile();
                otherTile.bottom = this;
                break;
            case direction.BOTTOM:
                bottom = new MazeTile();
                otherTile.top = this;
                break;
            case direction.LEFT:
                left = new MazeTile();
                otherTile.right = this;
                break;
            case direction.RIGHT:
                right = new MazeTile();
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
