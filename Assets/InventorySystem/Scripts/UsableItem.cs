using UnityEngine;

namespace Game.InventorySystem
{
    public class UsableItem : Item
    {
        public void Use()
        {
            Debug.Log("Item has been used!");
        }
    }

}