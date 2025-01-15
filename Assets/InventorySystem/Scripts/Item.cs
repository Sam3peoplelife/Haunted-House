using UnityEngine;

namespace Game.InventorySystem{
    public class Item : MonoBehaviour
    {
        [SerializeField] private Sprite itemSprite;
        [SerializeField] private GameObject pickupPromptPrefab;
        private GameObject currentPrompt;
        private bool canPickup = false;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                canPickup = true;
                //currentPrompt = Instantiate(pickupPromptPrefab, transform.position, Quaternion.identity);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                canPickup = false;
                if (currentPrompt != null)
                {
                    Destroy(currentPrompt);
                }
            }
        }

        private void Update()
        {
            if (canPickup && Input.GetKeyDown(KeyCode.E))
            {
                Inventory inventory = FindObjectOfType<Inventory>();
                if (inventory.AddItem(this))
                {
                    if (currentPrompt != null)
                    {
                        Destroy(currentPrompt);
                    }
                    // Instead of destroying, just disable the visuals and collider
                    spriteRenderer.enabled = false;
                    GetComponent<Collider2D>().enabled = false;
                    canPickup = false;
                }
            }
        }

        public Sprite GetSprite()
        {
            return itemSprite;
        }

        /// <summary>
        /// Shows the item in the world at a given position.
        /// Enables the sprite renderer and collider.
        /// </summary>
        /// <param name="position">The position to show the item.</param>
        public void ShowInWorld(Vector3 position)
        {
            transform.position = position;
            spriteRenderer.enabled = true;
            GetComponent<Collider2D>().enabled = true;
        }

        private void OnDestroy()
        {
            if (currentPrompt != null)
            {
                Destroy(currentPrompt);
            }
        }
    }
}
