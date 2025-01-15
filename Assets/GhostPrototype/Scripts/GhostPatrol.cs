using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ghost : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float waitTimeAtNode = 1f;
    [SerializeField] private float minDistanceToNode = 0.1f;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Debug")]
    [SerializeField] private bool showPathGizmos = false;

    private List<Node> currentPath = new List<Node>();
    private Node currentTargetNode;
    private Node currentNode;
    private bool isWaiting;
    private Transform playerTransform;
    private bool isChasing;
    private float currentSpeed;
    private Animator animator;

    private void Start()
    {
        currentSpeed = patrolSpeed;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
            Debug.LogWarning("Player not found! Make sure it has the 'Player' tag.");

        currentNode = AStarManager.Instance.FindNearestNode(transform.position);
        if (currentNode != null)
        {
            transform.position = currentNode.transform.position;
            StartCoroutine(PatrolRoutine());
        }
        else
        {
            Debug.LogError("Ghost: No starting node found!");
        }
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckForPlayer();
        MoveGhost();
    }

    private void CheckForPlayer()
    {
        if (playerTransform == null) return;

        Vector2 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= detectionRadius)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer.normalized, 
                distanceToPlayer, obstacleLayer);

            if (hit.collider == null)
            {
                if (!isChasing)
                {
                    isChasing = true;
                    currentSpeed = chaseSpeed;
                    StopAllCoroutines();
                    StartCoroutine(ChaseRoutine());
                }
            }
        }
        else if (isChasing)
        {
            isChasing = false;
            currentSpeed = patrolSpeed;
            StopAllCoroutines();
            StartCoroutine(PatrolRoutine());
        }
    }

    private void MoveGhost()
    {
        if (currentTargetNode != null && !isWaiting)
        {
            Vector2 direction = (currentTargetNode.transform.position - transform.position).normalized;
            transform.position += (Vector3)direction * currentSpeed * Time.deltaTime;

            // Update animation based on movement direction
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal movement is stronger
                if (direction.x > 0)
                    animator.SetFloat("Animations", 2); // move right
                else
                    animator.SetFloat("Animations", 3); // move left
            }
            else
            {
                // Vertical movement is stronger
                if (direction.y > 0)
                    animator.SetFloat("Animations", 0); // move up
                else
                    animator.SetFloat("Animations", 1); // move down
            }

            if (Vector2.Distance(transform.position, currentTargetNode.transform.position) < minDistanceToNode)
            {
                ReachNode();
            }
        }
    }

    private void ReachNode()
    {
        currentNode = currentTargetNode;

        if (currentPath.Count > 0)
        {
            currentTargetNode = currentPath[0];
            currentPath.RemoveAt(0);
        }
        else if (!isChasing)
        {
            currentTargetNode = null;
            StartCoroutine(WaitAtNode());
        }
    }

    private IEnumerator ChaseRoutine()
    {
        while (isChasing)
        {
            if (playerTransform != null)
            {
                Node playerNode = AStarManager.Instance.FindNearestNode(playerTransform.position);
                if (playerNode != null && playerNode != currentNode)
                {
                    currentPath = AStarManager.Instance.GeneratePath(currentNode, playerNode);
                    if (currentPath != null && currentPath.Count > 0)
                    {
                        currentTargetNode = currentPath[0];
                        currentPath.RemoveAt(0);
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator PatrolRoutine()
    {
        while (!isChasing)
        {
            if (currentPath.Count == 0 && !isWaiting)
            {
                Node targetNode = GetRandomNodeInHouse();
                if (targetNode != null && targetNode != currentNode)
                {
                    currentPath = AStarManager.Instance.GeneratePath(currentNode, targetNode);
                    if (currentPath != null && currentPath.Count > 0)
                    {
                        currentTargetNode = currentPath[0];
                        currentPath.RemoveAt(0);
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator WaitAtNode()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtNode);
        isWaiting = false;
    }

    private Node GetRandomNodeInHouse()
    {
        Node[] allNodes = FindObjectsOfType<Node>();
        if (allNodes.Length == 0) return null;

        return allNodes[Random.Range(0, allNodes.Length)];
    }

    private void OnDrawGizmos()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw path
        if (!showPathGizmos) return;
        
        Gizmos.color = Color.red;
        if (currentPath != null && currentPath.Count > 0)
        {
            Vector3 previousPosition = transform.position;
            
            if (currentTargetNode != null)
            {
                Gizmos.DrawLine(previousPosition, currentTargetNode.transform.position);
                previousPosition = currentTargetNode.transform.position;
            }
            
            foreach (Node node in currentPath)
            {
                Gizmos.DrawLine(previousPosition, node.transform.position);
                previousPosition = node.transform.position;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player catch event
            Debug.Log("Ghost caught the player!");
            // Add your game over logic here
        }
    }
}
