using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lamps : MonoBehaviour
{
    [SerializeField] private Light2D lampLight;
    private bool isOn = false;
    private bool canTurnOnLight = false;
    private void Start()
    {
        UpdateLightState();
    }

    private void Update()
    {
        if (canTurnOnLight && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed");
            isOn = !isOn; // Toggle light state
            UpdateLightState();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            canTurnOnLight = true;
        }
        // Check if it's the ghost
        else if (other.CompareTag("Ghost"))
        {
            isOn = false; // Turn off light
            UpdateLightState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            canTurnOnLight = false;
        }
    }

    private void UpdateLightState()
    {
        if (lampLight != null)
        {
            lampLight.intensity = isOn ? 1f : 0f;
        }
    }
}
