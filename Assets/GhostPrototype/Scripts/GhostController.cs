using UnityEngine;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    private Node currentNode;
    private Transform roomContainer; // Reference to the room's node container
    private List<Node> path = new List<Node>();
    
    [SerializeField] private float speed = 3f;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;
    
    private float waitTimer;
    private bool isWaiting;
    private Node[] roomNodes;

    public void Initialize(Node startNode, Transform roomNodeContainer)
    {
        currentNode = startNode;
        roomContainer = roomNodeContainer;
        roomNodes = roomContainer.GetComponentsInChildren<Node>();
        StartNewPath();
    }

    private void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                StartNewPath();
            }
            return;
        }

        if (path.Count == 0)
        {
            StartWaiting();
            return;
        }

        MoveAlongPath();
    }

    private void StartNewPath()
    {
        if (roomNodes == null || roomNodes.Length == 0) return;

        // Select random node in the room
        Node targetNode = roomNodes[Random.Range(0, roomNodes.Length)];
        while (targetNode == currentNode) // Ensure we don't pick the same node
        {
            targetNode = roomNodes[Random.Range(0, roomNodes.Length)];
        }
        
        // Generate path to target node using existing A* manager
        path = AStarManager.Instance.GeneratePath(currentNode, targetNode);
        
        if (path == null || path.Count == 0)
        {
            StartWaiting();
        }
    }

    private void MoveAlongPath()
    {
        if (path.Count == 0) return;

        Vector3 targetPosition = path[0].transform.position;
        targetPosition.z = transform.position.z; // Maintain z-position

        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            speed * Time.deltaTime
        );

        // Check if reached current node
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentNode = path[0];
            path.RemoveAt(0);

            if (path.Count == 0)
            {
                StartWaiting();
            }
        }
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    private void OnDrawGizmos()
    {
        if (path == null || path.Count == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (path[i] != null && path[i + 1] != null)
            {
                Gizmos.DrawLine(path[i].transform.position, path[i + 1].transform.position);
            }
        }
    }
}
