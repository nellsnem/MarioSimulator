using UnityEngine;

public class GroundCoin : MonoBehaviour
{
    // ==========================================
    // 1. CONSTANTS
    // ==========================================
    private const int COIN_SCORE = 100;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private bool _isCollected = false;

    // ==========================================
    // 3. MONOBEHAVIOUR METHODS
    // ==========================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isCollected) return; 
        if (!other.CompareTag("Player")) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        if (!CanCollect(player)) return;

        Collect();
    }

    // ==========================================
    // 4. PRIVATE METHODS
    // ==========================================
    private bool CanCollect(PlayerMovement player)
    {
        if (CompareTag("RedCoin")   && player.playerIndex == 1) return true;
        if (CompareTag("GreenCoin") && player.playerIndex == 2) return true;

        return false;
    }

    private void Collect()
    {
        _isCollected = true; 

        if (CompareTag("RedCoin"))
        {
            GameManager.Instance.AddRedCoin();
        }
        else if (CompareTag("GreenCoin"))
        {
            GameManager.Instance.AddGreenCoin();
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayCoin();
        }

        Destroy(gameObject);
    }
}