using System.Collections;
using UnityEngine;

public class BlockItem : MonoBehaviour
{
    // ==========================================
    // 1. PRIVATE FIELDS
    // ==========================================
    private Rigidbody2D      _rb;
    private CircleCollider2D _circleCollider;
    private BoxCollider2D    _boxCollider;
    private SpriteRenderer   _spriteRenderer;

    // ==========================================
    // 2. CONSTANTS
    // ==========================================
    private const float APPEAR_DELAY    = 0.25f;
    private const float APPEAR_DURATION = 0.5f;
    private const float APPEAR_HEIGHT   = 1f;

    // ==========================================
    // 3. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        _rb             = GetComponent<Rigidbody2D>();
        _circleCollider = GetComponent<CircleCollider2D>();
        _boxCollider    = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(AppearAnimation());
    }

    // ==========================================
    // 4. PRIVATE METHODS
    // ==========================================
    private void DisableComponents()
    {
        _rb.bodyType            = RigidbodyType2D.Kinematic;
        _circleCollider.enabled = false;
        _boxCollider.enabled    = false;
        _spriteRenderer.enabled = false;
    }

    private void EnableComponents()
    {
        _rb.bodyType            = RigidbodyType2D.Dynamic;
        _circleCollider.enabled = true;
        _boxCollider.enabled    = true;
    }

    // ==========================================
    // 5. COROUTINES
    // ==========================================
    private IEnumerator AppearAnimation()
    {
        DisableComponents();

        yield return new WaitForSeconds(APPEAR_DELAY);

        _spriteRenderer.enabled = true;

        yield return MoveUp();

        EnableComponents();
    }

    private IEnumerator MoveUp()
    {
        float   timeElapsed   = 0f;
        Vector3 startPosition = transform.position;
        Vector3 endPosition   = transform.position + Vector3.up * APPEAR_HEIGHT;

        while (timeElapsed < APPEAR_DURATION)
        {
            float progress     = timeElapsed / APPEAR_DURATION;
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            timeElapsed       += Time.deltaTime;
            yield return null;
        }
    }
}