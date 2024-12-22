using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerStats[] players;

    private void Start()
    {
        players = FindObjectsOfType<PlayerStats>();

        foreach (var player in players)
        {
            AdjustPlayerSettings(player);
        }
    }

    private void AdjustPlayerSettings(PlayerStats player)
    {
        player.sanityDrainRate *= 1 + (player.playerCount - 1) * 0.1f; //Increasing difficulty with number of players
        Debug.Log($"Player adjusted: Sanity Drain Rate {player.sanityDrainRate}");
    }
}
