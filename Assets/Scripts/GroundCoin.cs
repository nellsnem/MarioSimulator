using UnityEngine;

public class GroundCoin : MonoBehaviour
{
    // ==========================================
    // 1. CONSTANTS
    // ==========================================
    private const int COIN_SCORE = 100;

    // ==========================================
    // 2. MONOBEHAVIOUR METHODS
    // ==========================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        if (!CanCollect(player)) return;

        Collect();
    }

    // ==========================================
    // 3. PRIVATE METHODS
    // ==========================================
    private bool CanCollect(PlayerMovement player)
    {
        if (CompareTag("RedCoin")   && player.playerIndex == 1) return true;
        if (CompareTag("GreenCoin") && player.playerIndex == 2) return true;

        return false;
    }

    private void Collect()
    {
        GameManager.Instance.AddCoin();
        GameManager.Instance.AddScore(COIN_SCORE);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayCoin();
        }

        Destroy(gameObject);
    }
}