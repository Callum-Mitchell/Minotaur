using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//PathNode is used for implementing the A* pathfinding algorithm
//Sometime in the future, it may be beneficial to combine this class with the MazeNode class,
//as there's no definite reason they need to be separate.
public class PathNode
{
    public int row;
    public int column;
    public int gCost;
    public int hCost;
    public int fCost;
    public bool isDiscovered = false;
    public bool isClosed = false;
    public bool isTopAccessible;
    public bool isBottomAccessible;
    public bool isLeftAccessible;
    public bool isRightAccessible;
    public MazeConnectionDirection directionToApproachFrom; //In pathfinding, determines which previous node the current should be reached from
}

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

    //Pathfinding
    List<PathNode> nullNodesSet;
    List<PathNode> openNodesSet;
    List<PathNode> closedNodesSet;
    List <List<PathNode> > allNodesSet;

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
    private int targetRow, targetColumn;

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

        //Setup pathfinding nodes
        nullNodesSet = new List<PathNode>();
        openNodesSet = new List<PathNode>();
        closedNodesSet = new List<PathNode>();
        allNodesSet = new List<List<PathNode>>();
        for (int i = 0; i < columnCount; i++)
        {
            List<PathNode> pathNodeColumn = new List<PathNode>();
            for (int j = 0; j < rowCount; j++)
            {
                PathNode newNode = new PathNode();
                newNode.row = j;
                newNode.column = i;
                newNode.gCost = rowCount * columnCount;
                newNode.fCost = newNode.gCost;

                //Set obstacles
                newNode.isTopAccessible = (mazeLayout.tiles[i][j].top != null);
                newNode.isBottomAccessible = (mazeLayout.tiles[i][j].bottom != null);
                newNode.isLeftAccessible = (mazeLayout.tiles[i][j].left != null);
                newNode.isRightAccessible = (mazeLayout.tiles[i][j].right != null);

                pathNodeColumn.Add(newNode);
            }
            allNodesSet.Add(pathNodeColumn);
        }

        targetPath = new List<MinotaurFacingRotation>();
        StartCoroutine(listeningTimer());
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
        Vector3 distanceToPlayer = new Vector3(Mathf.Abs(currentRow - playerState.currentMazeRow) * mazeTileDepth, 0, Mathf.Abs(currentColumn - playerState.currentMazeColumn) * mazeTileWidth);
        float distanceMagnitude = distanceToPlayer.magnitude;
        float noiseLevelDifference = playerState.currentNoiseLevel - currentNoiseLevel;
        if(distanceMagnitude < noiseLevelDifference * 3f)
        {
            //Sound is loud/close enough to hear!
            //For now, just target the player's location
            targetRow = playerState.currentMazeRow;
            targetColumn = playerState.currentMazeColumn;
            if (distanceMagnitude + 10f < noiseLevelDifference * 3f)
            {
                //Sound is very, very noticeable to minotaur! He'll chase!
                movementState = MinotaurMovementState.CHASING;
            }
            else
            {
                movementState = MinotaurMovementState.INVESTIGATING;
            }
            findPathToTarget(targetRow, targetColumn);
        }
        else
        {
            movementState = MinotaurMovementState.ROAMING;
        }
        relocationDue = false;
    }

    void addNodeToOpenSet(PathNode node, PathNode endNode)
    {
        //Check G-cost by searching adjacent nodes
        node.gCost = rowCount * columnCount; //assume worst case: that all tiles must be visited to reach from start node
        PathNode lastNode;
        if (node.row < rowCount - 1)
        {
            lastNode = allNodesSet[node.column][node.row + 1];
            if (lastNode.isClosed && lastNode.isBottomAccessible && lastNode.gCost < node.gCost)
            {
                node.gCost = lastNode.gCost + 1;
                node.directionToApproachFrom = MazeConnectionDirection.TOP;
            }
        }
        if (node.row > 0)
        {
            lastNode = allNodesSet[node.column][node.row - 1];
            if (lastNode.isClosed && lastNode.isTopAccessible && lastNode.gCost < node.gCost)
            {
                node.gCost = lastNode.gCost + 1;
                node.directionToApproachFrom = MazeConnectionDirection.BOTTOM;
            }
        }
        if (node.column < columnCount - 1)
        {
            lastNode = allNodesSet[node.column + 1][node.row];
            if (lastNode.isClosed && lastNode.isLeftAccessible && lastNode.gCost < node.gCost)
            {
                node.gCost = lastNode.gCost + 1;
                node.directionToApproachFrom = MazeConnectionDirection.RIGHT;
            }
        }
        if (node.column > 0)
        {
            lastNode = allNodesSet[node.column - 1][node.row];
            if (lastNode.isClosed && lastNode.isRightAccessible && lastNode.gCost < node.gCost)
            {
                node.gCost = lastNode.gCost + 1;
                node.directionToApproachFrom = MazeConnectionDirection.LEFT;
            }
        }

        //Now obtain hCost (easy)
        node.hCost = Mathf.Abs(endNode.row - node.row) + Mathf.Abs(endNode.column - node.column);

        //Use to get fcost (unweighted for now)
        node.fCost = node.gCost + node.hCost;
        node.isDiscovered = true;

        //Add new node to open node set, then swap until it is in the correct place
        //(list should stay sorted by descending f-cost)
        openNodesSet.Add(node);
        int i = openNodesSet.Count - 1;
        while(i > 1 && openNodesSet[i - 1].fCost < node.fCost)
        {
            PathNode tmp = openNodesSet[i];
            openNodesSet[i] = openNodesSet[i - 1];
            openNodesSet[i - 1] = tmp;
            i--;
        }
    }

    void addNodeToClosedSet(PathNode node, PathNode lastNode, PathNode endNode)
    {
        if (!node.isClosed)
        {
            //Remove current node from open set and add to closed set
            openNodesSet.RemoveAt(openNodesSet.Count - 1);
            closedNodesSet.Add(node);
            node.isClosed = true;
        }
        node.isDiscovered = true;
        if (lastNode.row < node.row) node.directionToApproachFrom = MazeConnectionDirection.BOTTOM;
        else if (lastNode.row > node.row) node.directionToApproachFrom = MazeConnectionDirection.TOP;
        else if (lastNode.column < node.column) node.directionToApproachFrom = MazeConnectionDirection.LEFT;
        else node.directionToApproachFrom = MazeConnectionDirection.RIGHT;

        PathNode adjacentNode;
        for(int dirIndex = 0; dirIndex < 4; dirIndex++)
        {
            MazeConnectionDirection dirEnum = (MazeConnectionDirection)dirIndex;
            if (dirEnum == MazeConnectionDirection.RIGHT && node.column < columnCount - 1)
            {
                adjacentNode = allNodesSet[node.column + 1][node.row];
            }
            else if (dirEnum == MazeConnectionDirection.LEFT && node.column > 0)
            {
                adjacentNode = allNodesSet[node.column - 1][node.row];
            }
            else if (dirEnum == MazeConnectionDirection.TOP && node.row < rowCount - 1)
            {
                adjacentNode = allNodesSet[node.column][node.row + 1];
            }
            else if (dirEnum == MazeConnectionDirection.BOTTOM && node.row > 0)
            {
                adjacentNode = allNodesSet[node.column][node.row - 1];
            }
            else continue; //Means there is no adjacent tile in the direction being checked

            if (adjacentNode.isDiscovered && adjacentNode.gCost > node.gCost + 1)
            {
                //Need to update adjacent node's gcost and fcost
                adjacentNode.gCost = node.gCost + 1;
                adjacentNode.fCost = adjacentNode.gCost + adjacentNode.hCost;
                //Update the direction of approach for the adjacent node
                switch(dirEnum)
                {
                    case MazeConnectionDirection.BOTTOM: adjacentNode.directionToApproachFrom = MazeConnectionDirection.TOP; break;
                    case MazeConnectionDirection.TOP: adjacentNode.directionToApproachFrom = MazeConnectionDirection.BOTTOM; break;
                    case MazeConnectionDirection.LEFT: adjacentNode.directionToApproachFrom = MazeConnectionDirection.RIGHT; break;
                    case MazeConnectionDirection.RIGHT: adjacentNode.directionToApproachFrom = MazeConnectionDirection.LEFT; break;
                }
                if (!adjacentNode.isClosed)
                {
                    //Adjacent node is in open set. May need to find the node in the set, then update and re-sort set
                    if (adjacentNode.fCost < node.fCost)
                    {
                        for (int i = openNodesSet.Count - 1; i > 0; i--)
                        {
                            if (openNodesSet[i].row == adjacentNode.row && openNodesSet[i].column == adjacentNode.column)
                            {
                                //Found node
                                openNodesSet[i] = adjacentNode;
                                int j = i;
                                while (j < openNodesSet.Count - 1 && openNodesSet[j + 1].fCost > node.fCost)
                                {
                                    PathNode tmp = openNodesSet[j];
                                    openNodesSet[j] = openNodesSet[j + 1];
                                    openNodesSet[j + 1] = tmp;
                                    j++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //In closed set. Recursive addition to closed set needed.
                    addNodeToClosedSet(adjacentNode, node, endNode);
                }
            }
            else if(!adjacentNode.isDiscovered)
            {
                addNodeToOpenSet(adjacentNode, endNode);
            }
        }
    }

    void findPathToTarget(int targetRow, int targetColumn)
    {
        //pathfinding algorithm here... let's go for A*.

        //First, clear previous path info
        closedNodesSet.Clear();
        openNodesSet.Clear();

        //Next, reset the "all nodes" values (will have been affected by the last pathfind)
        for (int i = 0; i < columnCount; i++)
        {
            for (int j = 0; j < rowCount; j++)
            {                
                allNodesSet[i][j].gCost = rowCount * columnCount;
                allNodesSet[i][j].fCost = allNodesSet[i][j].gCost;
                allNodesSet[i][j].isClosed = false;
                allNodesSet[i][j].isDiscovered = false;
            }
        }

        //Get first and last node
        PathNode startNode = allNodesSet[currentColumn][currentRow];
        PathNode endNode = allNodesSet[targetColumn][targetRow];

        //Set gcost of start nodes
        startNode.gCost = 0;
        startNode.hCost = Mathf.Abs(endNode.column - startNode.column) + Mathf.Abs(endNode.row - startNode.row);
        startNode.fCost = startNode.gCost + startNode.hCost;
        startNode.isDiscovered = true;
        startNode.isClosed = true;
        closedNodesSet.Add(startNode);
        endNode.gCost = rowCount * columnCount;

        PathNode adjacentNode;
        //Add start-adjacent nodes to open set
        if(startNode.column < columnCount - 1)
        {
            addNodeToOpenSet(allNodesSet[currentColumn + 1][currentRow], endNode);
        }
        if (startNode.column > 0)
        {
            addNodeToOpenSet(allNodesSet[currentColumn - 1][currentRow], endNode);
        }
        if (startNode.row < rowCount - 1)
        {
            addNodeToOpenSet(allNodesSet[currentColumn][currentRow + 1], endNode);
        }
        if (startNode.row > 0)
        {
            addNodeToOpenSet(allNodesSet[currentColumn][currentRow - 1], endNode);
        }

        while (!endNode.isClosed)
        {
            //Get open node with smallest fCost
            PathNode nextNode = openNodesSet[openNodesSet.Count - 1];
            PathNode lastNode;
            if (nextNode.directionToApproachFrom == MazeConnectionDirection.TOP)
                lastNode = allNodesSet[nextNode.column][nextNode.row + 1];
            else if (nextNode.directionToApproachFrom == MazeConnectionDirection.BOTTOM)
                lastNode = allNodesSet[nextNode.column][nextNode.row - 1];
            else if (nextNode.directionToApproachFrom == MazeConnectionDirection.LEFT)
                lastNode = allNodesSet[nextNode.column - 1][nextNode.row];
            else
                lastNode = allNodesSet[nextNode.column + 1][nextNode.row];

            addNodeToClosedSet(nextNode, lastNode, endNode);
        }

        //Finally, take everything you have here and construct a path from the minotaur's position to the end position
        targetPath.Clear();
        PathNode currentNodeOnPath = endNode;
        while (currentNodeOnPath.row != startNode.row || currentNodeOnPath.column != startNode.column)
        {
            switch (currentNodeOnPath.directionToApproachFrom)
            {
                case MazeConnectionDirection.TOP:
                    if (currentNodeOnPath.row < rowCount - 1)
                    {
                        targetPath.Add(MinotaurFacingRotation.DOWN); //When *approaching* from the top, minotaur is *facing* downward
                        currentNodeOnPath = allNodesSet[currentNodeOnPath.column][currentNodeOnPath.row + 1];
                    }
                    break;
                case MazeConnectionDirection.BOTTOM:
                    if (currentNodeOnPath.row > 0)
                    {
                        targetPath.Add(MinotaurFacingRotation.UP); //etc
                        currentNodeOnPath = allNodesSet[currentNodeOnPath.column][currentNodeOnPath.row - 1];
                    }
                    break;
                case MazeConnectionDirection.LEFT:
                    if (currentNodeOnPath.column > 0)
                    {

                        targetPath.Add(MinotaurFacingRotation.RIGHT); //etc
                        currentNodeOnPath = allNodesSet[currentNodeOnPath.column - 1][currentNodeOnPath.row];
                    }
                    break;
                case MazeConnectionDirection.RIGHT:
                    if (currentNodeOnPath.column < columnCount - 1)
                    {
                        targetPath.Add(MinotaurFacingRotation.LEFT); //etc
                        currentNodeOnPath = allNodesSet[currentNodeOnPath.column + 1][currentNodeOnPath.row];
                    }
                    break;
            }
        } //Continue until start node is reached, and the path is complete!
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
        if (relocationDue)
        {
            locateSound();
            relocationDue = false;
        }
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
            case MinotaurMovementState.INVESTIGATING:
            case MinotaurMovementState.CHASING:
                if (targetPath.Count > 0)
                {
                    targetDirection = targetPath[targetPath.Count - 1];
                    targetPath.RemoveAt(targetPath.Count - 1);
                    StartCoroutine(changeDirection(targetDirection, true));
                }
                else
                {
                    relocationDue = true;
                    StartCoroutine(doNothing(0.5f));
                }
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
            int intLastRotation = Mathf.RoundToInt(lastRotation) + (turnRight ? 1 : -1);
            float nextRotation = lastRotation + rotationSpeed;
            int targetRotation = (int)target;
            if (intLastRotation > targetRotation && turnRight)
            {
                intLastRotation -= 360;
                lastRotation = (float)intLastRotation;
            }
            else if (intLastRotation < targetRotation && !turnRight)
            {
                intLastRotation += 360;
                lastRotation = (float)intLastRotation;
            }

            while ((turnRight && (intLastRotation < targetRotation))
                || (!turnRight && (intLastRotation > targetRotation)))
            {
                Body.Rotate(new Vector3(0, rotationSpeed, 0));
                lastRotation += rotationSpeed;
                intLastRotation = Mathf.RoundToInt(lastRotation);
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
        float targetXPos = ((currentRow + 1) * mazeTileDepth) + firstRowColumnPosition.x;
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
        Body.position = new Vector3(targetXPos, Body.position.y, Body.position.z);
        findNextAction();
    }
    IEnumerator MoveDown()
    {
        float targetXPos = ((currentRow - 1) * mazeTileDepth) + firstRowColumnPosition.x;
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
        Body.position = new Vector3(targetXPos, Body.position.y, Body.position.z);
        findNextAction();

    }
    IEnumerator MoveLeft()
    {
        float targetZPos = -((currentColumn - 1) * mazeTileDepth) + firstRowColumnPosition.z;
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
        Body.position = new Vector3(Body.position.x, Body.position.y, targetZPos);
        findNextAction();

    }
    IEnumerator MoveRight()
    {
        float targetZPos = -((currentColumn + 1) * mazeTileDepth) + firstRowColumnPosition.z;
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
        Body.position = new Vector3(Body.position.x, Body.position.y, targetZPos);
        findNextAction();

    }
}
