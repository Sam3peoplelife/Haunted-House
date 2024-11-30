using UnityEngine;
using UnityEngine.UI;

namespace Game.InventorySystem{
    public class Inventory : MonoBehaviour
    {
        [System.Serializable]
        public class InventorySlot
        {
            public Image slotImage;
            public Image frameImage;
            public Image useButtonImage;
            public Image dropButtonImage;
            public Item currentItem;
            public bool wasJustPickedUp; // Add flag for each slot
        }

        [SerializeField] private InventorySlot[] slots = new InventorySlot[4];
        private int selectedSlot = 0;

        void Start()
        {
            SelectSlot(0);
            UpdateAllSlots();
            
            // Initially hide all frames
            foreach (var slot in slots)
            {
                slot.frameImage.enabled = false;
                slot.wasJustPickedUp = false;
            }
            // Show only selected slot frame
            slots[selectedSlot].frameImage.enabled = true;
        }

        void Update()
        {
            scrollWheel();
            useAndDropHandler();
        }

        private void scrollWheel()
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                SelectSlot((selectedSlot + 1) % slots.Length);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                SelectSlot(selectedSlot == 0 ? slots.Length - 1 : selectedSlot - 1);
            }
        }

        private void useAndDropHandler()
        {
            // Handle use and drop inputs for selected slot
            if (slots[selectedSlot].currentItem != null)
            {
                if (Input.GetKeyDown(KeyCode.E) && !IsPlayerNearItem() && !slots[selectedSlot].wasJustPickedUp)
                {
                    UseSelectedItem();
                }
                if (Input.GetKeyDown(KeyCode.F))
                {
                    DropSelectedItem();
                }
            }
        }

        /// <summary>
        /// Checks if the player is near any item within a specified radius.
        /// </summary>
        /// <returns>True if the player is near an item, false otherwise.</returns>
        private bool IsPlayerNearItem()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(GameObject.FindGameObjectWithTag("Player").transform.position, 1f);
            foreach (Collider2D collider in colliders)
            {
                Item item = collider.GetComponent<Item>();
                if (item != null && collider.enabled)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds an item to the selected slot in the inventory, if the slot is empty.
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>True if the item was successfully added, false otherwise</returns>
        public bool AddItem(Item item)
        {
            if (slots[selectedSlot].currentItem == null)
            {
                slots[selectedSlot].currentItem = item;
                slots[selectedSlot].slotImage.sprite = item.GetSprite();
                slots[selectedSlot].slotImage.enabled = true;
                slots[selectedSlot].wasJustPickedUp = true; // Set the flag when item is picked up
                UpdateSlotUI(selectedSlot);
                
                // Reset the flag after a short delay
                StartCoroutine(ResetPickupFlag(selectedSlot));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets the <c>wasJustPickedUp</c> flag after a short delay.
        /// </summary>
        /// <param name="slotIndex">The index of the slot whose flag should be reset</param>
        private System.Collections.IEnumerator ResetPickupFlag(int slotIndex)
        {
            yield return new WaitForSeconds(0.1f);
            slots[slotIndex].wasJustPickedUp = false;
        }

        /// <summary>
        /// Selects a slot in the inventory, hiding the previous selection and
        /// showing the new selection. Also highlights the selected slot with
        /// a yellow frame.
        /// </summary>
        /// <param name="index">The index of the slot to select</param>
        private void SelectSlot(int index)
        {
            // Hide previous slot frame
            slots[selectedSlot].frameImage.enabled = false;
            
            // Select new slot
            selectedSlot = index;
            
            // Show and highlight new slot frame
            slots[selectedSlot].frameImage.enabled = true;
            slots[selectedSlot].frameImage.color = Color.yellow;
        }

        /// <summary>
        /// Updates a slot's UI by enabling/disabling the use and drop buttons
        /// based on whether the slot has an item in it.
        /// </summary>
        /// <param name="slotIndex">The index of the slot to update</param>
        private void UpdateSlotUI(int slotIndex)
        {
            bool hasItem = slots[slotIndex].currentItem != null;
            slots[slotIndex].useButtonImage.enabled = hasItem;
            slots[slotIndex].dropButtonImage.enabled = hasItem;
        }

        /// <summary>
        /// Updates all the slots in the inventory, enabling/disabling the use and drop buttons
        /// based on whether each slot has an item in it.
        /// </summary>
        private void UpdateAllSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                UpdateSlotUI(i);
            }
        }

        /// <summary>
        /// Uses the item currently selected in the inventory, if it is a UsableItem.
        /// </summary>
        private void UseSelectedItem()
        {
            if (slots[selectedSlot].currentItem is UsableItem usableItem)
            {
                usableItem.Use();
            }
        }

        /// <summary>
        /// Drops the item currently selected in the inventory.
        /// </summary>
        /// <remarks>
        /// The item is spawned in the world at the player's position.
        /// </remarks>
        private void DropSelectedItem()
        {
            if (slots[selectedSlot].currentItem != null)
            {
                slots[selectedSlot].currentItem.ShowInWorld(GameObject.FindGameObjectWithTag("Player").transform.position);
                slots[selectedSlot].currentItem = null;
                UpdateSlotUI(selectedSlot);
            }
        }
    }
}

