using UnityEngine;


public class DeathBarrier : MonoBehaviour
{
    // ==========================================
    // 1. MONOBEHAVIOUR METHODS
    // ==========================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            KillPlayer(other);
        }
        else
        {
            Destroy(other.gameObject);
        }
    }

    // ==========================================
    // 2. PRIVATE METHODS
    // ==========================================
    private void KillPlayer(Collider2D playerCollider)
    {
        PlayerMovement player = playerCollider.GetComponent<PlayerMovement>();

        if (player == null || player.IsDead)
        {
            return;
        }

        player.Die();
    }
}