using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minotaur : MonoBehaviour {

    //caching
    private Transform Body;
    public GameObject playerStateManagerObject;
    private PlayerStateManager playerState;
    public GameObject mazeLayoutObject;
    private MazeLayout mazeLayout;
    //Behavioral config
    public float secondsPerRelocation = 3f;
    //public float normalAcceleration = 0.05f; //global units per fixedUpdate squared (will save this stuff for later prototypes)
    public float roamingMoveSpeed = 0.1f; //global units per fixedUpdate
    public float roamingRotationSpeed = 2f; //degrees per fixedUpdate
    public float investigatingMoveSpeed = 0.18f;
    public float investigatingRotationSpeed = 3f;
    public float chasingMoveSpeed = 0.3f;
    public float chasingRotationSpeed = 4f;
    public float chargingStartMoveSpeed = 1f;
    public float chargingAcceleration = 0.01f; //Minotaur accelerates as it continues charging

    //Navigation parameters
    public float mazeTileWidth = 6f, mazeTileDepth = 6f;
    public int rowCount = 20, columnCount = 20;
    public int minStartingRow = 0, maxStartingRow = 19;
    public int minStartingColumn = 0, maxStartingColumn = 19;

    //State values
    private int currentRow, currentColumn;
    private MinotaurFacingRotation facingDirection;
    private List<MinotaurFacingRotation> targetPath; //contains all 1-tile movements required to reach target tile (last movement first in list)
    private MinotaurMovementState movementState;
    private float currentMovementSpeed = 0f;
    private float chargeSpeedOffset = 0f;
    private bool relocationDue = false;

    //Noise level units are in decibels. Higher = louder noises made by minotaur.
    //The minotaur can only hear noises with a coded noise level above its own
    //Each difference of 1 means can be heard from a distance of 3 in-game units
    private float roamingNoiseLevel = 0f;
    private float investigatingNoiseLevel = 3f;
    private float chargingNoiseLevel = 25f;
    private float chasingNoiseLevel = 7f;
    private float standingNoiseLevel = -5f;
    private float currentNoiseLevel;

    private List<float> directionToYRotation; //lookup table
    public Vector3 firstRowColumnPosition;
    private Vector3 targetPosition;
    // Use this for initialization
    void Start() {
        Body = transform;
    }
    //Called separately from start
    public void setup()
    {
        playerState = playerStateManagerObject.GetComponent<PlayerStateManager>();
        mazeLayout = mazeLayoutObject.GetComponent<MazeLayout>();
        //Generate a random starting tile within a possible tile range
        int startCol = Random.Range(minStartingColumn, maxStartingColumn);
        int startRow = Random.Range(minStartingRow, maxStartingRow);
        //Use to set starting position
        transform.position = new Vector3(firstRowColumnPosition.x + (startRow * mazeTileDepth), firstRowColumnPosition.y, firstRowColumnPosition.z - (startCol * mazeTileWidth));
        currentColumn = startCol;
        currentRow = startRow;
        //Randomize starting rotation
        int startFaceDirectionIndex = Random.Range(0, 3);
        switch (startFaceDirectionIndex)
        {
            case 0: facingDirection = MinotaurFacingRotation.UP; break;
            case 1: facingDirection = MinotaurFacingRotation.LEFT; break;
            case 2: facingDirection = MinotaurFacingRotation.DOWN; break;
            case 3: facingDirection = MinotaurFacingRotation.RIGHT; break;
        }
        Body.rotation = Quaternion.Euler(0, (float)facingDirection, 0);

        //Default movement state to roaming
        movementState = MinotaurMovementState.ROAMING;
        currentNoiseLevel = roamingNoiseLevel;
        relocationDue = false;
        StartCoroutine(doNothing(1f));
    }
    IEnumerator doNothing(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        findNextAction();
    }

    // Update is called once per frame
    void FixedUpdate() {

    }

    //(TODO) function for determining location of a source sound
    //For now, this will just be the player. In future builds, this will grow to include other sounds
    //(Another function will have to be called first 
    void locateSound()
    {

    }

    void findPathToTarget(int targetRow, int targetColumn)
    {

    }

    //Coroutine that signals time to re-listen for sounds (eg of the player every x seconds
    IEnumerator listeningTimer()
    {
        //At some point, we'll have the minotaur estimate the player's location with some margin for error.
        //For now, though, the minotaur will simply locate the player if a) they are within a certain range,
        //and b) they are making noise above a certain threshold (give each movement option its own threshold)
        
        yield return new WaitForSeconds(secondsPerRelocation);
        relocationDue = true;
        StartCoroutine(listeningTimer());
    }

    //Determines which way the minotaur should move next
    void findNextAction()
    {
        //Check: are we due for a player relocation?

        switch(movementState)
        {
            case MinotaurMovementState.ROAMING:
                //ROAMING: pick a direction to move from the current tile (must be minotaur-accessible)
                MazeTile currentTile = mazeLayout.tiles[currentColumn][currentRow];
                //Start with a random direction that isn't behind the minotaur (weigh towards forward). If free, select.
                int startingRotation;
                float startingValue = Random.Range(0f, 1f);
                if (startingValue < 0.2f)
                {
                    //90 degree clockwise turn
                    startingRotation = (int)facingDirection + 90;
                }
                else if(startingValue < 0.8f)
                {
                    //straight ahead
                    startingRotation = (int)facingDirection;
                }
                else
                {
                    //90 degree counterclockwise turn
                    startingRotation = (int)facingDirection - 90;
                }
                if (startingRotation < 0) startingRotation += 360;
                else if (startingRotation > 270) startingRotation -= 360;
                MinotaurFacingRotation startingDirection = (MinotaurFacingRotation)startingRotation;
                MinotaurFacingRotation targetDirection = startingDirection;
                do
                {
                    switch(targetDirection)
                    {
                        case MinotaurFacingRotation.UP:
                            if (currentTile.top != null)
                            {
                                StartCoroutine(changeDirection(targetDirection, true));
                                return;
                            }
                            break;
                        case MinotaurFacingRotation.RIGHT:
                            if (currentTile.right != null)
                            {
                                StartCoroutine(changeDirection(targetDirection, true));
                                return;
                            }
                            break;
                        case MinotaurFacingRotation.LEFT:
                            if (currentTile.left != null)
                            {
                                StartCoroutine(changeDirection(targetDirection, true));
                                return;
                            }
                            break;
                        case MinotaurFacingRotation.DOWN:
                            if (currentTile.bottom != null)
                            {
                                StartCoroutine(changeDirection(targetDirection, true));
                                return;
                            }
                            break;
                    }
                    //Next: try path to right of minotaur, weighing towards forward
                    int newTargetDirection = (int)targetDirection + (startingValue < 0.2f ? -90 : 90);
                    if (newTargetDirection > 270) newTargetDirection -= 360;
                    else if (newTargetDirection < 0) newTargetDirection += 360;
                    targetDirection = (MinotaurFacingRotation)newTargetDirection;
                    
                } while (startingDirection != targetDirection);
                //Minotaur is trapped on 1 tile (wow! How'd you pull that off, player?!)
                StartCoroutine(doNothing(1f));
                break;
            default:
                //standing or unknown state
                StartCoroutine(doNothing(1f));
                break;
        }
    }

    //Coroutine for rotating the minotaur
    IEnumerator changeDirection(MinotaurFacingRotation target, bool moveAfter)
    {
        //Skip all the rotation if already facing the right direction
        if (target != facingDirection)
        {
            //Determine rotation speed
            float rotationSpeed;
            switch (movementState)
            {
                case MinotaurMovementState.INVESTIGATING: rotationSpeed = investigatingRotationSpeed; break;
                case MinotaurMovementState.CHASING: rotationSpeed = chasingRotationSpeed; break;
                case MinotaurMovementState.ROAMING:
                default:
                    rotationSpeed = roamingRotationSpeed;
                    break;
            }

            //Determine whether to rotate left or right. Only turn right up to 90 degrees
            bool turnRight =
                (facingDirection == MinotaurFacingRotation.UP && target == MinotaurFacingRotation.RIGHT
                || facingDirection == MinotaurFacingRotation.RIGHT && target == MinotaurFacingRotation.DOWN
                || facingDirection == MinotaurFacingRotation.DOWN && target == MinotaurFacingRotation.LEFT
                || facingDirection == MinotaurFacingRotation.LEFT && target == MinotaurFacingRotation.UP);
            if (!turnRight)
            {
                rotationSpeed = -rotationSpeed;
            }
            float lastRotation = Body.eulerAngles.y;
            float nextRotation = lastRotation + rotationSpeed;
            float targetRotation = (float)target;
            while ((lastRotation < targetRotation - 0.0001f && nextRotation <= targetRotation - 0.0001f)
                || (lastRotation > targetRotation + 0.0001f && nextRotation >= targetRotation + 0.0001f))
            {
                Body.Rotate(new Vector3(0, rotationSpeed, 0));
                lastRotation = Body.eulerAngles.y;
                nextRotation = lastRotation + rotationSpeed;
                yield return new WaitForFixedUpdate();
            }

            facingDirection = target;
        }
        //If moveAfter is true, move in the newly-faced direction
        if (moveAfter)
        {
            switch (target)
            {
                case MinotaurFacingRotation.UP: StartCoroutine(MoveUp()); break;
                case MinotaurFacingRotation.DOWN: StartCoroutine(MoveDown()); break;
                case MinotaurFacingRotation.LEFT: StartCoroutine(MoveLeft()); break;
                case MinotaurFacingRotation.RIGHT: StartCoroutine(MoveRight()); break;
            }
        }
    }

    //Movement routines covering roaming, investigating, chasing, charging, etc
    IEnumerator MoveUp()
    {
        float targetXPos = Body.position.x + mazeTileDepth;
        float moveSpeed;
        switch (movementState)
        {
            case MinotaurMovementState.INVESTIGATING: moveSpeed = investigatingMoveSpeed; break;
            case MinotaurMovementState.CHASING: moveSpeed = chasingMoveSpeed; break;
            case MinotaurMovementState.CHARGING: moveSpeed = chargingStartMoveSpeed + chargeSpeedOffset; break;
            case MinotaurMovementState.ROAMING:
            default:
                moveSpeed = roamingMoveSpeed;
                break;
        }
        while (Body.position.x < targetXPos)
        {
            Body.position = new Vector3(Body.position.x + moveSpeed, Body.position.y, Body.position.z);
            if (movementState == MinotaurMovementState.CHARGING)
            {
                moveSpeed += chargingAcceleration;
                chargeSpeedOffset += chargingAcceleration;
            }
            yield return new WaitForFixedUpdate();
        }
        currentRow += 1;
        findNextAction();
    }
    IEnumerator MoveDown()
    {
        float targetXPos = Body.position.x - mazeTileDepth;
        float moveSpeed;
        switch (movementState)
        {
            case MinotaurMovementState.INVESTIGATING: moveSpeed = investigatingMoveSpeed; break;
            case MinotaurMovementState.CHASING: moveSpeed = chasingMoveSpeed; break;
            case MinotaurMovementState.CHARGING: moveSpeed = chargingStartMoveSpeed + chargeSpeedOffset; break;
            case MinotaurMovementState.ROAMING:
            default:
                moveSpeed = roamingMoveSpeed;
                break;
        }
        while (Body.position.x > targetXPos)
        {
            Body.position = new Vector3(Body.position.x - moveSpeed, Body.position.y, Body.position.z);
            if (movementState == MinotaurMovementState.CHARGING)
            {
                moveSpeed += chargingAcceleration;
                chargeSpeedOffset += chargingAcceleration;
            }
            yield return new WaitForFixedUpdate();
        }
        currentRow -= 1;
        findNextAction();

    }
    IEnumerator MoveLeft()
    {
        float targetZPos = Body.position.z + mazeTileWidth;
        float moveSpeed;
        switch (movementState)
        {
            case MinotaurMovementState.INVESTIGATING: moveSpeed = investigatingMoveSpeed; break;
            case MinotaurMovementState.CHASING: moveSpeed = chasingMoveSpeed; break;
            case MinotaurMovementState.CHARGING: moveSpeed = chargingStartMoveSpeed + chargeSpeedOffset; break;
            case MinotaurMovementState.ROAMING:
            default:
                moveSpeed = roamingMoveSpeed;
                break;
        }
        while (Body.position.z < targetZPos)
        {
            Body.position = new Vector3(Body.position.x, Body.position.y, Body.position.z + moveSpeed);
            if (movementState == MinotaurMovementState.CHARGING)
            {
                moveSpeed += chargingAcceleration;
                chargeSpeedOffset += chargingAcceleration;
            }
            yield return new WaitForFixedUpdate();
        }
        currentColumn -= 1;
        findNextAction();

    }
    IEnumerator MoveRight()
    {
        float targetZPos = Body.position.z - mazeTileWidth;
        float moveSpeed;
        switch (movementState)
        {
            case MinotaurMovementState.INVESTIGATING: moveSpeed = investigatingMoveSpeed; break;
            case MinotaurMovementState.CHASING: moveSpeed = chasingMoveSpeed; break;
            case MinotaurMovementState.CHARGING: moveSpeed = chargingStartMoveSpeed + chargeSpeedOffset; break;
            case MinotaurMovementState.ROAMING:
            default:
                moveSpeed = roamingMoveSpeed;
                break;
        }
        while (Body.position.z > targetZPos)
        {
            Body.position = new Vector3(Body.position.x, Body.position.y, Body.position.z - moveSpeed);
            if (movementState == MinotaurMovementState.CHARGING)
            {
                moveSpeed += chargingAcceleration;
                chargeSpeedOffset += chargingAcceleration;
            }
            yield return new WaitForFixedUpdate();
        }
        currentColumn += 1;
        findNextAction();

    }
}
