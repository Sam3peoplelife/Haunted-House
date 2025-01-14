using UnityEngine;
using System.Collections.Generic;

public class GhostSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private NodePlacer nodePlacer;
    
    private GameObject currentGhost;

    private void OnEnable()
    {
        NodePlacer.OnNodesPlaced += OnNodesPlaced;
    }

    private void OnDisable()
    {
        NodePlacer.OnNodesPlaced -= OnNodesPlaced;
    }

    private void OnNodesPlaced()
    {
        SpawnGhost();
    }

    private void Start()
    {
        SpawnGhost();
    }

    public void SpawnGhost()
    {
        if (currentGhost != null)
        {
            Destroy(currentGhost);
        }

        NodePlacer.RoomData randomRoom = GetRandomRoom();
        
        // Add more detailed debug information
        if (randomRoom == null)
        {
            //Debug.LogError("Random room is null!");
            return;
        }

        //Debug.Log($"Selected Room: {randomRoom.roomName}, Node Count: {randomRoom.nodes.Count}");

        if (randomRoom.nodes == null || randomRoom.nodes.Count == 0)
        {
            //Debug.LogError($"No nodes found in room: {randomRoom.roomName}");
            return;
        }

        Node spawnNode = GetRandomNode(randomRoom);

        if (spawnNode == null)
        {
            //Debug.LogError($"Failed to find valid spawn node in room: {randomRoom.roomName}");
            return;
        }

        // Add position debugging
        //Debug.Log($"Spawning ghost at position: {spawnNode.transform.position}");
        currentGhost = Instantiate(ghostPrefab, spawnNode.transform.position, Quaternion.identity);
    }

    private NodePlacer.RoomData GetRandomRoom()
    {
        List<NodePlacer.RoomData> rooms = nodePlacer.rooms;
        
        if (rooms == null)
        {
            //Debug.LogError("Rooms list is null!");
            return null;
        }

        if (rooms.Count == 0)
        {
            //Debug.LogError("Rooms list is empty!");
            return null;
        }

        // Debug information about available rooms
        //Debug.Log($"Total number of rooms: {rooms.Count}");
        foreach (var room in rooms)
        {
            //Debug.Log($"Room: {room.roomName}, Nodes: {room.nodes.Count}");
        }

        int randomIndex = Random.Range(0, rooms.Count);
        NodePlacer.RoomData selectedRoom = rooms[randomIndex];
        
        //Debug.Log($"Selected room index: {randomIndex}, Room name: {selectedRoom.roomName}");
        return selectedRoom;
    }

    private Node GetRandomNode(NodePlacer.RoomData room)
    {
        if (room.nodes == null)
        {
            //Debug.LogError($"Nodes list is null for room: {room.roomName}");
            return null;
        }

        if (room.nodes.Count == 0)
        {
            //Debug.LogError($"No nodes available in room: {room.roomName}");
            return null;
        }

        int randomIndex = Random.Range(0, room.nodes.Count);
        Node selectedNode = room.nodes[randomIndex];
        
        //Debug.Log($"Selected node index: {randomIndex} in room: {room.roomName}");
        return selectedNode;
    }

    public void RespawnGhost()
    {
        SpawnGhost();
    }
}
