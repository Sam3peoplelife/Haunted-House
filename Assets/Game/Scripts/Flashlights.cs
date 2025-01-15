using UnityEngine;
using UnityEngine.Rendering.Universal; // For Light2D

namespace Game.InventorySystem
{
    public class Flashlights : Item
    {
        [SerializeField] private Light2D playerLight;
        private bool isOn = false;
        private float batteryLife = 100f;
        private float batteryDrainRate = 5f;
        
        public void Use()
        {
            if (batteryLife <= 0 && !isOn)
            {
                Debug.Log("Flashlight battery is dead!");
                return;
            }
            isOn = !isOn;
            playerLight.intensity = isOn ? 1f : 0f;
            Debug.Log(isOn ? "Flashlight turned ON" : "Flashlight turned OFF");
        }
    }
}
