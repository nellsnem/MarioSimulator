using System.Collections;
using UnityEngine;

public class DeathAnimation : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    public Sprite deadSprite;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private SpriteRenderer _spriteRenderer;

    // ==========================================
    // 3. CONSTANTS
    // ==========================================
    private const float DEATH_DURATION      = 3f;
    private const float DEATH_JUMP_SPEED    = 8f;
    private const float DEATH_GRAVITY       = -30f;
    private const int   DEATH_SORTING_ORDER = 20;

    // ==========================================
    // 4. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        PlayDeathAnimation();
    }

    // ==========================================
    // 5. PRIVATE METHODS
    // ==========================================
    private void PlayDeathAnimation()
    {
        SetupSprite();
        DisablePhysics();
        StartCoroutine(Animate());
    }

    private void SetupSprite()
    {
        _spriteRenderer.enabled      = true;
        _spriteRenderer.sortingOrder = DEATH_SORTING_ORDER;

        if (deadSprite != null)
        {
            _spriteRenderer.sprite = deadSprite;
        }
    }

    private void DisablePhysics()
    {
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        if (TryGetComponent(out Rigidbody2D rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (TryGetComponent(out PlayerMovement pm))
        {
            pm.enabled = false;
        }

        if (TryGetComponent(out EntityMovement em))
        {
            em.enabled = false;
        }

        if (TryGetComponent(out GoombaScript gs))
        {
            gs.enabled = false;
        }
    }

    // ==========================================
    // 6. COROUTINES
    // ==========================================
    private IEnumerator Animate()
    {
        float elapsed    = 0f;
        Vector3 velocity = Vector3.up * DEATH_JUMP_SPEED;

        while (elapsed < DEATH_DURATION)
        {
            transform.position += velocity * Time.deltaTime;
            velocity.y         += DEATH_GRAVITY * Time.deltaTime;
            elapsed            += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}