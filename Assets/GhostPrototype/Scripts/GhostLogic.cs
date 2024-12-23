using UnityEngine;
using Game.Global.System;
using UnityEngine.Tilemaps;

public class GhostLogic : Singleton<GhostLogic>
{
    public enum GhostState
    {
        Idle,
        Wandering,
        Hunting,
        Chasing,
        Returning
    }

    [System.Serializable]
    public class Room
    {
        public Tilemap roomTilemap;
    }

    [Header("Room Settings")]
    [SerializeField] private Room[] rooms;
    [SerializeField] private Transform ghostTransform;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 4f;
    [SerializeField] private SpriteRenderer ghostSprite;

    [Header("Hunt Settings")]
    [SerializeField] private float huntDuration = 60f;
    [SerializeField] private float huntCooldown = 120f;
    [SerializeField] private float huntSpeed = 15f;
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private float searchDuration = 5f;
    [SerializeField] private LayerMask visionBlockingLayers;
    [SerializeField] private Transform playerTransform;

    [Header("Debug")]
    [SerializeField] private bool showDebugVisuals = true;

    private Room currentRoom;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float waitTimer = 0f;

    private GhostState currentState = GhostState.Idle;
    private float huntTimer;
    private float searchTimer;
    private float cooldownTimer;
    private Vector3 lastKnownPlayerPosition;
    private Room startingRoom;

    void Start()
    {
        SelectRandomRoomAndMoveGhost();
        startingRoom = currentRoom;
        SetNewTargetPosition();
        currentState = GhostState.Wandering;
    }

    void Update()
    {
        UpdateTimers();
        UpdateGhostState();
        
        if (showDebugVisuals)
        {
            DrawDebugVisuals();
        }
    }

    private void UpdateTimers()
    {
        if (currentState == GhostState.Hunting || currentState == GhostState.Chasing)
        {
            huntTimer -= Time.deltaTime;
            if (huntTimer <= 0)
            {
                EndHunt();
            }
        }
        else if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (currentState == GhostState.Chasing && !CanSeePlayer())
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                currentState = GhostState.Hunting;
            }
        }
    }

    private void UpdateGhostState()
    {
        switch (currentState)
        {
            case GhostState.Wandering:
                UpdateWandering();
                break;
            case GhostState.Hunting:
                UpdateHunting();
                break;
            case GhostState.Chasing:
                UpdateChasing();
                break;
            case GhostState.Returning:
                UpdateReturning();
                break;
        }
    }

    private bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector3 directionToPlayer = playerTransform.position - ghostTransform.position;
        float angleToPlayer = Vector3.Angle(ghostTransform.right, directionToPlayer);

        if (angleToPlayer > fieldOfView * 0.5f) return false;
        if (directionToPlayer.magnitude > visionRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            ghostTransform.position,
            directionToPlayer.normalized,
            visionRange,
            visionBlockingLayers
        );

        if (hit.collider != null)
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    public void StartHunt()
    {
        if (cooldownTimer > 0) return;

        currentState = GhostState.Hunting;
        huntTimer = huntDuration;
        moveSpeed = huntSpeed;
    }

    private void EndHunt()
    {
        currentState = GhostState.Returning;
        moveSpeed = moveSpeed / 2;
        cooldownTimer = huntCooldown;
        SetTargetToStartingRoom();
    }

    private void UpdateWandering()
    {
        if (!isMoving)
        {
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                return;
            }
            SetNewTargetPosition();
        }
        else
        {
            MoveTowardsTarget();
        }
    }

    private void UpdateHunting()
    {
        if (CanSeePlayer())
        {
            currentState = GhostState.Chasing;
            lastKnownPlayerPosition = playerTransform.position;
            searchTimer = searchDuration;
            return;
        }

        if (!isMoving || Vector3.Distance(ghostTransform.position, targetPosition) < 0.1f)
        {
            targetPosition = GetRandomPositionInHouse();
            isMoving = true;
        }

        MoveTowardsTarget();
    }

    private void UpdateChasing()
    {
        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = playerTransform.position;
            searchTimer = searchDuration;
            targetPosition = lastKnownPlayerPosition;
            isMoving = true;
        }

        MoveTowardsTarget();
    }

    private void UpdateReturning()
    {
        if (Vector3.Distance(ghostTransform.position, targetPosition) < 0.1f)
        {
            currentRoom = startingRoom;
            currentState = GhostState.Wandering;
            moveSpeed = moveSpeed * 2;
            return;
        }

        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 moveDirection = (targetPosition - ghostTransform.position).normalized;
        ghostTransform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (ghostSprite != null)
        {
            if (moveDirection.x > 0)
            {
                ghostSprite.flipX = false;
            }
            else if (moveDirection.x < 0)
            {
                ghostSprite.flipX = true;
            }
        }

        if (Vector3.Distance(ghostTransform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            if (currentState == GhostState.Wandering)
            {
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }
        }
    }

    private void SetNewTargetPosition()
    {
        targetPosition = GetRandomPositionInCurrentRoom();
        isMoving = true;
    }

    private void SetTargetToStartingRoom()
    {
        targetPosition = GetRandomPositionInRoom(startingRoom);
        isMoving = true;
    }

    private void SelectRandomRoomAndMoveGhost()
    {
        if (rooms == null || rooms.Length == 0) return;

        int randomIndex = Random.Range(0, rooms.Length);
        currentRoom = rooms[randomIndex];
        
        if (ghostTransform != null)
        {
            ghostTransform.position = GetRandomPositionInCurrentRoom();
        }
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    private Vector3 GetRandomPositionInCurrentRoom()
    {
        return GetRandomPositionInRoom(currentRoom);
    }

    private Vector3 GetRandomPositionInHouse()
    {
        Room randomRoom = rooms[Random.Range(0, rooms.Length)];
        return GetRandomPositionInRoom(randomRoom);
    }

    private Vector3 GetRandomPositionInRoom(Room room)
    {
        if (room == null || room.roomTilemap == null)
        {
            return Vector3.zero;
        }

        BoundsInt bounds = room.roomTilemap.cellBounds;
        
        for (int attempts = 0; attempts < 100; attempts++)
        {
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            
            Vector3Int tilePosition = new Vector3Int(x, y, 0);
            if (room.roomTilemap.HasTile(tilePosition))
            {
                return room.roomTilemap.GetCellCenterWorld(tilePosition);
            }
        }

        return Vector3.zero;
    }

    private void DrawDebugVisuals()
    {
        if (!showDebugVisuals) return;

        float halfFOV = fieldOfView * 0.5f;
        Vector3 leftRayDirection = Quaternion.Euler(0, 0, halfFOV) * ghostTransform.right;
        Vector3 rightRayDirection = Quaternion.Euler(0, 0, -halfFOV) * ghostTransform.right;

        Debug.DrawRay(ghostTransform.position, leftRayDirection * visionRange, Color.yellow);
        Debug.DrawRay(ghostTransform.position, rightRayDirection * visionRange, Color.yellow);

        if (currentState == GhostState.Chasing)
        {
            Debug.DrawLine(ghostTransform.position, lastKnownPlayerPosition, Color.red);
        }
    }
}
