using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour {

    public GameObject playerObject;
    private Transform playerTransform;
    public int currentMazeRow = 0;
    public int currentMazeColumn = 0;
    public float firstRowPos;
    public float firstColumnPos;
    public float mazeTileDepth = 6f;
    public float mazeTileWidth = 6f;

    //Noise level units are in decibels. Higher = minotaur can hear from farther away.
    //0 means minotaur cannot hear sound even from right next to the player
    //Each increase by 1 means can be heard from a distance of 3 in-game units
    //Note that the minotaur is also making noise - the minotaur's noise level will be subtracted from this value and that of other sounds
    private float walkingNoiseLevel = 16f;
    private float RunningNoiseLevel = 23f;
    private float crouchingNoiseLevel = 9f;
    private float standingNoiseLevel = 0f;

    public float currentNoiseLevel;

    // Use this for initialization
    void Start () {
        playerTransform = playerObject.transform;
        currentMazeRow = Mathf.RoundToInt((playerTransform.position.x - firstRowPos) / mazeTileDepth);
        currentMazeColumn = Mathf.RoundToInt(-(playerTransform.position.z - firstColumnPos) / mazeTileWidth);
        currentNoiseLevel = standingNoiseLevel;
    }

    // Update is called once per frame
    void FixedUpdate () {
        currentMazeRow = Mathf.RoundToInt((playerTransform.position.x - firstRowPos) / mazeTileDepth);
        currentMazeColumn = Mathf.RoundToInt(-(playerTransform.position.z - firstColumnPos) / mazeTileWidth);
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)
            || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)
            || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                //crouching
                currentNoiseLevel = crouchingNoiseLevel;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                //Running
                currentNoiseLevel = RunningNoiseLevel;
            }
            else
            {
                //Walking
                currentNoiseLevel = walkingNoiseLevel;
            }
        }
        else
        {
            //standing in place
            currentNoiseLevel = standingNoiseLevel;
        }
    }
}
