using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ItemToSpawn
    {
        public GameObject prefab;
        public int quantity = 1;
    }

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<ItemToSpawn> itemsToSpawn = new List<ItemToSpawn>();
    [SerializeField] private float yOffset = 0.5f; // Offset to place items above the tile

    private void Start()
    {
        SpawnItems();
    }

    private void SpawnItems()
    {
        if (tilemap == null || itemsToSpawn.Count == 0)
            return;

        // Get all valid tile positions
        List<Vector3Int> validTilePositions = GetValidTilePositions();

        if (validTilePositions.Count == 0)
            return;

        // Spawn each item
        foreach (ItemToSpawn item in itemsToSpawn)
        {
            for (int i = 0; i < item.quantity; i++)
            {
                if (validTilePositions.Count == 0)
                    return; // No more valid positions

                // Get random position from valid positions
                int randomIndex = Random.Range(0, validTilePositions.Count);
                Vector3Int tilePosition = validTilePositions[randomIndex];
                
                // Convert tile position to world position
                Vector3 worldPosition = tilemap.GetCellCenterWorld(tilePosition);
                worldPosition.y += yOffset; // Add offset to place item above the tile

                // Spawn the item
                GameObject spawnedItem = Instantiate(item.prefab, worldPosition, Quaternion.identity);
                spawnedItem.transform.parent = transform; // Parent to this object for organization

                // Remove used position from valid positions
                validTilePositions.RemoveAt(randomIndex);
            }
        }
    }

    private List<Vector3Int> GetValidTilePositions()
    {
        List<Vector3Int> validPositions = new List<Vector3Int>();
        BoundsInt bounds = tilemap.cellBounds;

        // Iterate through all tiles in the tilemap
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                
                // Check if there's a tile at this position
                if (tilemap.HasTile(tilePosition))
                {
                    validPositions.Add(tilePosition);
                }
            }
        }

        return validPositions;
    }

    // Editor method to manually trigger spawn
    public void SpawnItemsManually()
    {
        SpawnItems();
    }

    // Editor method to clear spawned items
    public void ClearSpawnedItems()
    {
        // Destroy all child objects (spawned items)
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
