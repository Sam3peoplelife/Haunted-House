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
        private Animator animator;


        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            currentSpeed = normalSpeed;
            currentStamina = maxStamina;
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            // Handle input
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;

            // Handle sprinting
            HandleSprinting();
            
            // Handle stamina regeneration
            HandleStaminaRegen();
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            if (movement.magnitude > 0)
            {
                // Determine direction based on movement
                if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                {
                    // Horizontal movement is stronger
                    if (movement.x > 0)
                        animator.SetFloat("Animations", 6); // move_right
                    else
                        animator.SetFloat("Animations", 7); // move_left
                }
                else
                {
                    // Vertical movement is stronger
                    if (movement.y > 0)
                        animator.SetFloat("Animations", 4); // move_up
                    else
                        animator.SetFloat("Animations", 5); // move_down
                }
            }
            else
            {
                // Get the current animation value to determine which idle to use
                float currentAnim = animator.GetFloat("Animations");
                
                // Set corresponding idle animation based on last movement
                if (currentAnim == 4) // was moving up
                    animator.SetFloat("Animations", 0); // idle_up
                else if (currentAnim == 5) // was moving down
                    animator.SetFloat("Animations", 1); // idle_down
                else if (currentAnim == 6) // was moving right
                    animator.SetFloat("Animations", 2); // idle_right
                else if (currentAnim == 7) // was moving left
                    animator.SetFloat("Animations", 3); // idle_left
            }
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
