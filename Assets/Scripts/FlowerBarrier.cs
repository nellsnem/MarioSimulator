using System.Collections;
using UnityEngine;

public class FlowerBarrier : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    [Header("Settings")]
    public float dropDistance  = 2f;
    public float dropSpeed     = 3f;
    public float fadeDuration  = 0.4f;

    [Header("Sprites")]
    public Sprite spriteIdle;
    public Sprite spriteOpen;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private bool _isOpen = false;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;

    // ==========================================
    // 3. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider       = GetComponent<Collider2D>();

        if (_spriteRenderer != null && spriteIdle != null)
        {
            _spriteRenderer.sprite = spriteIdle;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isOpen) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player == null) return;

        player.Hit();
    }

    // ==========================================
    // 4. PUBLIC METHODS
    // ==========================================
    public void Open()
    {
        if (_isOpen) return;

        _isOpen = true;
        StartCoroutine(OpenSequence());
    }

    // ==========================================
    // 5. COROUTINES
    // ==========================================
    private IEnumerator OpenSequence()
    {
        if (_spriteRenderer != null && spriteOpen != null)
        {
            _spriteRenderer.sprite = spriteOpen;
        }

        yield return null;
        yield return null;

        if (_collider != null)
        {
            _collider.enabled = false;
        }

        Vector3 startPos  = transform.position;
        Vector3 targetPos = startPos + Vector3.down * dropDistance;

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, targetPos, dropSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        float elapsed    = 0f;
        Color startColor = _spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha           = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        _spriteRenderer.sortingOrder = -1;
        _spriteRenderer.color        = new Color(1f, 1f, 1f, 0f);
    }
}