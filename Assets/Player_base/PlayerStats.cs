using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Base Stats")]
    public float sanity = 0;
    public float gameTime = 0;
    public int evidenceCount = 0;
    public int playerCount = 1;
    public float flashlightCharge = 100;
    public float sanityDrainRate = 1.5f;
    public float playerSpeed = 0.75f;
    public bool dropItemsOnDeath = true;
    public bool flashlightEnabled = true;

    private void Start()
    {
        Debug.Log($"Player Initialized. Sanity: {sanity}, Speed: {playerSpeed * 100}%");
    }

    public void UpdateSanity(float amount)
    {
        sanity += amount;
        sanity = Mathf.Clamp(sanity, 0, 100);
        Debug.Log($"Sanity updated: {sanity}");
    }

    public void UpdateFlashlightCharge(float amount)
    {
        flashlightCharge += amount;
        flashlightCharge = Mathf.Clamp(flashlightCharge, 0, 100);
        Debug.Log($"Flashlight charge updated: {flashlightCharge}");
    }

    public float GetDifficultyMultiplier()
    {
        return 1 + (playerCount - 1) * 0.1f;
    }
}