using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minotaur : MonoBehaviour {

    //caching
    private Transform Body;
    public GameObject playerStateManagerObject;
    private PlayerStateManager playerState;
    //Behavioral config
    public float secondsPerRelocation = 5f;
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
    private MinotaurFacingRotation facingDirection;
    private MinotaurMovementState movementState;
    private float currentMovementSpeed = 0;
    private float chargeSpeedOffset = 0f;

    private List<float> directionToYRotation; //lookup table
    public Vector3 firstRowColumnPosition;
    private Vector3 targetPosition;
    // Use this for initialization
    void Start() {
        playerState = playerStateManagerObject.GetComponent<PlayerStateManager>();
        //Generate a random starting tile within a possible tile range
        int startCol = Random.Range(minStartingColumn, maxStartingColumn);
        int startRow = Random.Range(minStartingRow, maxStartingRow);
        //Use to set starting position
        transform.position = new Vector3(firstRowColumnPosition.x + (startRow * mazeTileDepth), firstRowColumnPosition.y, firstRowColumnPosition.z - (startCol * mazeTileWidth));
        //Randomize starting rotation
        int startFaceDirectionIndex = Random.Range(0, 3);
        switch (startFaceDirectionIndex)
        {
            case 0: facingDirection = MinotaurFacingRotation.UP; break;
            case 1: facingDirection = MinotaurFacingRotation.LEFT; break;
            case 2: facingDirection = MinotaurFacingRotation.DOWN; break;
            case 3: facingDirection = MinotaurFacingRotation.RIGHT; break;
        }
        transform.rotation = Quaternion.Euler(0, (float)facingDirection, 0);

    }

    // Update is called once per frame
    void FixedUpdate() {

    }

    //(TODO) function for determining location of player

    //Coroutine that calls decision to relocate player every x seconds
    IEnumerator locatePlayer()
    {
        //At some point, we'll have the minotaur estimate the player's location with some margin for error.
        //For now, though, the minotaur will simply locate the player if a) they are within a certain range,
        //and b) they are making noise above a certain threshold (give each movement option its own threshold)

        yield return new WaitForSeconds(secondsPerRelocation);
        StartCoroutine(locatePlayer());
    }

    //
    void findNextAction()
    {

    }

    //Coroutine for rotating the minotaur
    IEnumerator changeDirection(MinotaurFacingRotation target)
    {
        //Determine rotation speed
        float rotationSpeed;
        switch(movementState)
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
        if(!turnRight)
        {
            rotationSpeed = -rotationSpeed;
        }
        float lastRotation = Body.eulerAngles.y;
        float nextRotation = lastRotation + rotationSpeed;
        float targetRotation = (float)target;
        while ((lastRotation < targetRotation && nextRotation <= targetRotation)
            || (lastRotation > targetRotation && nextRotation >= targetRotation))
        {
            Body.Rotate(new Vector3(0, rotationSpeed, 0));
            lastRotation = Body.eulerAngles.y;
            nextRotation = lastRotation + rotationSpeed;
            yield return new WaitForFixedUpdate();
        } 

    }

    //Movement routine covering roaming, investigating, chasing, charging, etc
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
            Body.Translate(new Vector3(moveSpeed, 0, 0));
            if (movementState == MinotaurMovementState.CHARGING)
            {
                moveSpeed += chargingAcceleration;
                chargeSpeedOffset += chargingAcceleration;
            }
            yield return new WaitForFixedUpdate();
        }

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
            Body.Translate(new Vector3(-moveSpeed, 0, 0));
            if (movementState == MinotaurMovementState.CHARGING)
            {
                moveSpeed += chargingAcceleration;
                chargeSpeedOffset += chargingAcceleration;
            }
            yield return new WaitForFixedUpdate();
        }

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
            Body.Translate(new Vector3(0, moveSpeed, 0));
            if (movementState == MinotaurMovementState.CHARGING)
            {
                moveSpeed += chargingAcceleration;
                chargeSpeedOffset += chargingAcceleration;
            }
            yield return new WaitForFixedUpdate();
        }

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
            Body.Translate(new Vector3(0, -moveSpeed, 0));
            if (movementState == MinotaurMovementState.CHARGING)
            {
                moveSpeed += chargingAcceleration;
                chargeSpeedOffset += chargingAcceleration;
            }
            yield return new WaitForFixedUpdate();
        }

    }
}
