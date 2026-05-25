using UnityEngine;

public class GoombaScript : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    public Sprite flatSprite;

    // ==========================================
    // 2. MONOBEHAVIOUR METHODS
    // ==========================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (!GetComponent<Collider2D>().enabled)
        {
            return;
        }

        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();

        if (player.IsStarpower)
        {
            Hit();
            return;
        }

        if (collision.transform.DotTest(transform, 0.5f))
        {
            StompByPlayer(collision.gameObject);
            return;
        }

        player.Hit();
    }

    // ==========================================
    // 3. PUBLIC METHODS
    // ==========================================
    public void Flatten()
    {
        GetComponent<Collider2D>().enabled = false;

        if (TryGetComponent(out EntityMovement em))
        {
            em.enabled = false;
        }

        if (TryGetComponent(out AnimatedSprite anim))
        {
            anim.enabled = false;
        }

        GetComponent<SpriteRenderer>().sprite = flatSprite;
        Destroy(gameObject, 0.5f);
    }

    public void Hit()
    {
        GameManager.Instance.AddScore(300);

        if (TryGetComponent(out DeathAnimation death))
        {
            death.enabled = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // 4. PRIVATE METHODS
    // ==========================================
    private void StompByPlayer(GameObject playerObject)
    {
        Flatten();
        GameManager.Instance.AddScore(300);

        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f);
        }
    }
}