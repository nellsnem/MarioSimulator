using System.Collections;
using UnityEngine;
 
public class BlockHit : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    public GameObject item;
    public Sprite emptyBlock;
    public int maxHits = 1;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private bool _animating;

    // ==========================================
    // 3. CONSTANTS
    // ==========================================
    private const float HIT_ANIM_HEIGHT   = 0.5f;
    private const float HIT_ANIM_DURATION = 0.125f;

    // ==========================================
    // 4. MONOBEHAVIOUR METHODS
    // ==========================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_animating || maxHits == 0 || !collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (!collision.transform.DotTest(transform, 0.5f))
        {
            Hit();
        }
    }

    // ==========================================
    // 5. PRIVATE METHODS
    // ==========================================
    private void Hit()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;

        maxHits--;

        if (maxHits == 0)
        {
            spriteRenderer.sprite = emptyBlock;
        }

        if (item != null)
        {
            Instantiate(item, transform.position, Quaternion.identity);
        }

        StartCoroutine(Animate());
    }

    // ==========================================
    // 6. COROUTINES
    // ==========================================
    private IEnumerator Animate()
    {
        _animating = true;

        Vector3 restingPosition  = transform.localPosition;
        Vector3 animatedPosition = restingPosition + Vector3.up * HIT_ANIM_HEIGHT;

        yield return Move(restingPosition, animatedPosition);
        yield return Move(animatedPosition, restingPosition);

        _animating = false;
    }

    private IEnumerator Move(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < HIT_ANIM_DURATION)
        {
            float progress = elapsed / HIT_ANIM_DURATION;
            transform.localPosition = Vector3.Lerp(from, to, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = to;
    }
}