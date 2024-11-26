using UnityEngine;

namespace Game.Movement.Logic
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float normalSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float currentSpeed;
        
        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina;
        [SerializeField] private float staminaDrainRate = 20f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float staminaRegenDelay = 1f;
        
        private Rigidbody2D rb;
        private Vector2 movement;
        private bool isSprinting;
        private float staminaRegenTimer;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            currentSpeed = normalSpeed;
            currentStamina = maxStamina;
        }

        void Update()
        {
            // Handle input
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            // Handle sprinting
            HandleSprinting();
            
            // Handle stamina regeneration
            HandleStaminaRegen();
        }

        void FixedUpdate()
        {
            rb.MovePosition(rb.position + movement.normalized * currentSpeed * Time.fixedDeltaTime);
        }

        private void HandleSprinting()
        {
            // Check if player is trying to sprint
            if (Input.GetKey(KeyCode.LeftShift) && movement.magnitude > 0 && currentStamina > 0)
            {
                isSprinting = true;
                currentSpeed = sprintSpeed;
                
                // Drain stamina while sprinting
                currentStamina -= staminaDrainRate * Time.deltaTime;
                currentStamina = Mathf.Max(0, currentStamina); // Prevent negative stamina
                
                // Reset regen timer
                staminaRegenTimer = staminaRegenDelay;
            }
            else
            {
                isSprinting = false;
                currentSpeed = normalSpeed;
            }
        }

        private void HandleStaminaRegen()
        {
            // Only regenerate stamina when not sprinting
            if (!isSprinting)
            {
                if (staminaRegenTimer > 0)
                {
                    staminaRegenTimer -= Time.deltaTime;
                }
                else if (currentStamina < maxStamina)
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    currentStamina = Mathf.Min(currentStamina, maxStamina); // Cap at max stamina
                }
            }
        }

        public float GetStaminaPercentage()
        {
            return currentStamina / maxStamina;
        }
    }
}
