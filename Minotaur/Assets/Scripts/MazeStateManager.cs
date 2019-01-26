using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeStateManager : MonoBehaviour
{

    public MazeGenerator mazeGenerator;
    int mazeWidth = 15; //Maze width in tiles
    int mazeDepth = 15;
    float tileWidth = 8f; //measured in global Unity transform units
    float tileHeight = 8f;

    public MazeLayout mazeLayout;


    // Use this for initialization
    void Start()
    {

        //Generate maze

    }

    // FixedUpdate is called once per delta time
    void FixedUpdate()
    {

    }
}
