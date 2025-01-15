using UnityEngine;

public class RotateLight : MonoBehaviour
{
    private UnityEngine.Rendering.Universal.Light2D light2D;
    private Vector2 movement;
    private float rotationSpeed = 10;

    void Start()
    {
        light2D = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero) // Check if there's any input
        {
            float targetAngle = 0f;
            // Determine which direction has the strongest input
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                // Horizontal movement is stronger
                if (movement.x > 0)
                    targetAngle = -90f;  // Right
                else
                    targetAngle = 90f;  // Left
            }
            else
            {
                // Vertical movement is stronger
                if (movement.y > 0)
                    targetAngle = 0f;  // Up
                else
                    targetAngle = 180f;  // Down
            }
            
            // Set outer angle to match inner angle
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                transform.rotation = Quaternion.Lerp(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
        }
    }
}

