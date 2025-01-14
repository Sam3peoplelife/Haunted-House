using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Game.Global.System;

public class NodePlacer : Singleton<NodePlacer>
{
    [System.Serializable]
    public class RoomData
    {
        public string roomName;
        public Tilemap roomTilemap;
        public Transform nodesParent; // This will be the child of masterNodesParent
        public List<Node> nodes = new List<Node>();
    }

    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private float nodeConnectionRadius = 2f;
    [SerializeField] private Transform masterNodesParent; // Single parent for all rooms
    [SerializeField] public List<RoomData> rooms = new List<RoomData>();

    public static event System.Action OnNodesPlaced;

    private void Start()
    {
        InitializeRoomContainers();
        PlaceNodesInAllRooms();
        OnNodesPlaced?.Invoke();
    }

    private void InitializeRoomContainers()
    {
        // Create master parent if it doesn't exist
        if (masterNodesParent == null)
        {
            GameObject masterParent = new GameObject("Master_Nodes_Parent");
            masterNodesParent = masterParent.transform;
            masterNodesParent.parent = transform;
        }

        // Create container for each room under the master parent
        foreach (var room in rooms)
        {
            if (room.nodesParent == null)
            {
                GameObject roomContainer = new GameObject($"Room_{room.roomName}_Nodes");
                room.nodesParent = roomContainer.transform;
                room.nodesParent.parent = masterNodesParent; // Set as child of master parent
            }
        }
    }

    private void PlaceNodesInAllRooms()
    {
        if (nodePrefab == null) return;

        foreach (var room in rooms)
        {
            if (room.roomTilemap == null) continue;
            PlaceNodesInRoom(room);
        }

        // Connect nodes within each room
        foreach (var room in rooms)
        {
            ConnectNodes(room.nodes);
        }

        // Connect nodes between rooms
        ConnectRooms();
    }

    private void PlaceNodesInRoom(RoomData room)
    {
        BoundsInt bounds = room.roomTilemap.cellBounds;
        float subdivisions = 2f; // Each tile will be divided into 2x2 sections
        float offset = 1f / subdivisions;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                
                if (room.roomTilemap.HasTile(tilePosition))
                {
                    // Get the world position of the tile center
                    Vector3 baseTilePosition = room.roomTilemap.GetCellCenterWorld(tilePosition);

                    // Place nodes in a grid within each tile
                    for (float subX = -0.5f + (offset/2); subX <= 0.5f - (offset/2); subX += offset)
                    {
                        for (float subY = -0.5f + (offset/2); subY <= 0.5f - (offset/2); subY += offset)
                        {
                            Vector3 nodePosition = new Vector3(
                                baseTilePosition.x + subX,
                                baseTilePosition.y + subY,
                                baseTilePosition.z
                            );

                            GameObject nodeObj = Instantiate(nodePrefab, nodePosition, Quaternion.identity);
                            nodeObj.transform.parent = room.nodesParent;
                            nodeObj.name = $"Node_{x}_{y}_{subX}_{subY}";
                            
                            Node node = nodeObj.GetComponent<Node>();
                            if (node != null)
                            {
                                room.nodes.Add(node);
                            }
                        }
                    }
                }
            }
        }
    }


    private void ConnectNodes(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            node.connections = new List<Node>();

            foreach (Node otherNode in nodes)
            {
                if (node == otherNode) continue;

                float distance = Vector2.Distance(node.transform.position, otherNode.transform.position);
                
                if (distance <= nodeConnectionRadius && HasLineOfSight(node.transform.position, otherNode.transform.position))
                {
                    node.connections.Add(otherNode);
                }
            }
        }
    }

    private void ConnectRooms()
    {
        // Connect nodes between rooms if they are close enough and have line of sight
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                ConnectRoomNodes(rooms[i], rooms[j]);
            }
        }
    }

    private void ConnectRoomNodes(RoomData room1, RoomData room2)
    {
        foreach (Node node1 in room1.nodes)
        {
            foreach (Node node2 in room2.nodes)
            {
                float distance = Vector2.Distance(node1.transform.position, node2.transform.position);
                
                if (distance <= nodeConnectionRadius && HasLineOfSight(node1.transform.position, node2.transform.position))
                {
                    node1.connections.Add(node2);
                    node2.connections.Add(node1);
                }
            }
        }
    }

    private bool HasLineOfSight(Vector2 start, Vector2 end)
    {
        RaycastHit2D hit = Physics2D.Raycast(start, (end - start).normalized, Vector2.Distance(start, end));
        return hit.collider == null;
    }

    // Editor visualization
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        foreach (var room in rooms)
        {
            // Draw nodes
            Gizmos.color = Color.yellow;
            foreach (Node node in room.nodes)
            {
                if (node == null) continue;
                Gizmos.DrawWireSphere(node.transform.position, 0.3f);
                
                // Draw connections
                if (node.connections != null)
                {
                    foreach (Node connection in node.connections)
                    {
                        if (connection != null)
                        {
                            // Different colors for intra-room and inter-room connections
                            Gizmos.color = IsInSameRoom(node, connection) ? Color.blue : Color.red;
                            Gizmos.DrawLine(node.transform.position, connection.transform.position);
                        }
                    }
                }
            }
        }
    }

    private bool IsInSameRoom(Node node1, Node node2)
    {
        return node1.transform.parent == node2.transform.parent;
    }

    // Editor utility method to clear all nodes
    public void ClearAllNodes()
    {
        if (masterNodesParent != null)
        {
            DestroyImmediate(masterNodesParent.gameObject);
            masterNodesParent = null;
        }

        foreach (var room in rooms)
        {
            room.nodes.Clear();
            room.nodesParent = null;
        }
    }

    // Editor utility method to regenerate all nodes
    public void RegenerateAllNodes()
    {
        ClearAllNodes();
        InitializeRoomContainers();
        PlaceNodesInAllRooms();
    }
}
