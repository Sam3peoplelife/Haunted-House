using UnityEngine;
using Game.Global.System;
using UnityEngine.Tilemaps;

public class GhostLogic : Singleton<GhostLogic>
{
    [System.Serializable]
    public class Room
    {
        public Tilemap roomTilemap;
    }

    [SerializeField] private Room[] rooms;
    [SerializeField] private Transform ghostTransform;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 4f;
    [SerializeField] private SpriteRenderer ghostSprite; // Reference to ghost's sprite renderer
    
    private Room currentRoom;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float waitTimer = 0f;

    void Start()
    {
        SelectRandomRoomAndMoveGhost();
        SetNewTargetPosition();
    }

    void Update()
    {
        if (!isMoving)
        {
            // If we're waiting, count down the timer
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                return;
            }
            
            // Time to move again
            SetNewTargetPosition();
        }
        else
        {
            // Move towards target position
            Vector3 moveDirection = (targetPosition - ghostTransform.position).normalized;
            ghostTransform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Update ghost facing direction using sprite flip
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

            // Check if we've reached the target (with small threshold)
            if (Vector3.Distance(ghostTransform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }
        }
    }

    private void SetNewTargetPosition()
    {
        targetPosition = GetRandomPositionInCurrentRoom();
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

    public Vector3 GetRandomPositionInCurrentRoom()
    {
        if (currentRoom == null || currentRoom.roomTilemap == null)
        {
            return Vector3.zero;
        }

        BoundsInt bounds = currentRoom.roomTilemap.cellBounds;
        
        for (int attempts = 0; attempts < 100; attempts++)
        {
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            
            Vector3Int tilePosition = new Vector3Int(x, y, 0);
            if (currentRoom.roomTilemap.HasTile(tilePosition))
            {
                return currentRoom.roomTilemap.GetCellCenterWorld(tilePosition);
            }
        }

        return Vector3.zero;
    }
}