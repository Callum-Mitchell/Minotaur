using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MazeConnectionDirection
{
    TOP, LEFT, BOTTOM, RIGHT
};

public enum MinotaurFacingRotation
{
    UP = 180, LEFT = 90, DOWN = 0, RIGHT = 270
}

public enum MinotaurMovementState
{
    STOPPED, ROAMING, INVESTIGATING, CHASING, CHARGING
}