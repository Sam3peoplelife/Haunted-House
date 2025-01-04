using UnityEngine;
using System.Collections.Generic;

public class GhostSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Transform masterNodesParent;

    private void Awake()
    {
        Debug.Log("GhostSpawner: Awake");
    }

    private void Start()
    {
        Debug.Log("GhostSpawner: Start - Attempting to spawn ghost");
        SpawnGhostInRandomRoom();
    }

    private void SpawnGhostInRandomRoom()
    {
        // Check master nodes parent
        if (masterNodesParent == null)
        {
            Debug.LogError("GhostSpawner: Master nodes parent is not assigned!");
            return;
        }
        Debug.Log($"GhostSpawner: Found master nodes parent: {masterNodesParent.name}");

        // Check ghost prefab
        if (ghostPrefab == null)
        {
            Debug.LogError("GhostSpawner: Ghost prefab is not assigned!");
            return;
        }
        Debug.Log("GhostSpawner: Ghost prefab is assigned");

        // Get room containers
        int roomCount = masterNodesParent.childCount;
        if (roomCount == 0)
        {
            Debug.LogError("GhostSpawner: No rooms found in master parent!");
            return;
        }
        Debug.Log($"GhostSpawner: Found {roomCount} rooms");

        // Get all room containers
        Transform[] roomContainers = new Transform[roomCount];
        for (int i = 0; i < roomCount; i++)
        {
            roomContainers[i] = masterNodesParent.GetChild(i);
            Debug.Log($"GhostSpawner: Room {i}: {roomContainers[i].name}");
        }

        // Select random room
        int randomRoomIndex = Random.Range(0, roomContainers.Length);
        Transform selectedRoom = roomContainers[randomRoomIndex];
        Debug.Log($"GhostSpawner: Selected room: {selectedRoom.name}");

        // Get nodes in selected room
        Node[] roomNodes = selectedRoom.GetComponentsInChildren<Node>();
        if (roomNodes == null || roomNodes.Length == 0)
        {
            Debug.LogError($"GhostSpawner: No nodes found in room {selectedRoom.name}!");
            return;
        }
        Debug.Log($"GhostSpawner: Found {roomNodes.Length} nodes in selected room");

        // Select random node
        int randomNodeIndex = Random.Range(0, roomNodes.Length);
        Node spawnNode = roomNodes[randomNodeIndex];
        Debug.Log($"GhostSpawner: Selected node {randomNodeIndex} for spawning");

        try
        {
            // Spawn ghost
            GameObject ghostObj = Instantiate(ghostPrefab, spawnNode.transform.position, Quaternion.identity);
            ghostObj.name = $"Ghost_{selectedRoom.name}";
            Debug.Log($"GhostSpawner: Ghost spawned at position {ghostObj.transform.position}");

            // Initialize ghost controller
            GhostController ghost = ghostObj.GetComponent<GhostController>();
            if (ghost != null)
            {
                ghost.Initialize(spawnNode, selectedRoom);
                Debug.Log("GhostSpawner: Ghost controller initialized successfully");
            }
            else
            {
                Debug.LogError("GhostSpawner: GhostController component not found on spawned ghost!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GhostSpawner: Error spawning ghost: {e.Message}\n{e.StackTrace}");
        }
    }

    // Editor utility method
    public void TriggerSpawn()
    {
        Debug.Log("GhostSpawner: Manual spawn triggered");
        SpawnGhostInRandomRoom();
    }
}