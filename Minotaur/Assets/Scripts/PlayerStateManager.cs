using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour {

    public GameObject playerObject;
    public int currentMazeRow = 0;
    public int currentMazeColumn = 0;
    public float firstRowPos;
    public float firstColumnPos;
    public float mazeTileDepth = 6f;
    public float mazeTileWidth = 6f;

    //Noise level units are in decibels. Higher = minotaur can hear from farther away.
    //0 means minotaur cannot hear sound even from right next to the player
    //Each increase by 1 means can be heard from a distance of 3 in-game units
    private float walkingNoiseLevel = 15f;
    private float RunningNoiseLevel = 25f;
    private float sneakingNoiseLevel = 5f;

    private float currentNoiseLevel;

    // Use this for initialization
    void Start () {
        currentMazeRow = Mathf.RoundToInt((playerObject.transform.position.x - firstRowPos) / mazeTileDepth);
        currentMazeColumn = Mathf.RoundToInt((playerObject.transform.position.y - firstColumnPos) / mazeTileWidth);
    }

    // Update is called once per frame
    void FixedUpdate () {
        currentMazeRow = Mathf.RoundToInt((playerObject.transform.position.x - firstRowPos) / mazeTileDepth);
        currentMazeColumn = Mathf.RoundToInt((playerObject.transform.position.y - firstColumnPos) / mazeTileWidth);
    }
}
