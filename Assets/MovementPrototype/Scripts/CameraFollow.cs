using UnityEngine;

namespace Game.Global.System{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;        // Reference to the player's transform
        [SerializeField] private float smoothSpeed = 5f;  // How smoothly the camera follows the player
        [SerializeField] private Vector3 offset;         // Offset distance between the player and camera

        private void Start()
        {
            // If no target is assigned, try to find the player
            if (target == null)
            {
                // Attempt to find a GameObject with a Player tag
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            // Store the initial offset if target is found
            if (target != null)
            {
                offset = transform.position - target.position;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Calculate the desired position
            Vector3 desiredPosition = target.position + offset;
            
            // Smoothly move the camera towards the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }
}
