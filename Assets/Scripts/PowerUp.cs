using UnityEngine;

public class PowerUp : MonoBehaviour
{
    // ==========================================
    // 1. ENUMS
    // ==========================================
    public enum PowerUpType
    {
        Coin,
        ExtraLife,
        MagicMushroom,
        Starpower,
    }

    // ==========================================
    // 2. PUBLIC FIELDS
    // ==========================================
    public PowerUpType type;

    // ==========================================
    // 3. MONOBEHAVIOUR METHODS
    // ==========================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (other.TryGetComponent(out PlayerMovement player))
        {
            Collect(player);
        }
    }

    // ==========================================
    // 4. PRIVATE METHODS
    // ==========================================
    private void Collect(PlayerMovement player)
    {
        DisableCollider();
        ApplyEffect(player);
        Destroy(gameObject);
    }

    private void DisableCollider()
    {
        if (TryGetComponent(out Collider2D col))
        {
            col.enabled = false;
        }
    }

    private void ApplyEffect(PlayerMovement player)
    {
        switch (type)
        {
            case PowerUpType.Coin:
                CollectCoin();
                break;

            case PowerUpType.ExtraLife:
                CollectExtraLife();
                break;

            case PowerUpType.MagicMushroom:
                CollectMushroom(player);
                break;

            case PowerUpType.Starpower:
                CollectStarpower(player);
                break;
        }
    }

    private void CollectCoin()
    {
        GameManager.Instance?.AddCoin();
    }

    private void CollectExtraLife()
    {
        GameManager.Instance?.AddLife();
        MusicManager.Instance?.PlayLife();
    }

    private void CollectMushroom(PlayerMovement player)
    {
        GameManager.Instance?.AddScore(150);
        player.Grow();
        MusicManager.Instance?.PlayGrow();
    }

    private void CollectStarpower(PlayerMovement player)
    {
        GameManager.Instance?.AddScore(500);
        player.Starpower();
    }
}