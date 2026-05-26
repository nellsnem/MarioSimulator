using System.Collections;
using UnityEngine;

 
public class BlockItem : MonoBehaviour
{
    // ==========================================
    // 1. CONSTANTS
    // ==========================================
    private const float APPEAR_DELAY    = 0.25f;
    private const float APPEAR_DURATION = 0.5f;
    private const float APPEAR_HEIGHT   = 1f;

    // ==========================================
    // 2. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Start()
    {
        StartCoroutine(AppearAnimation());
    }

    // ==========================================
    // 3. COROUTINES
    // ==========================================
    private IEnumerator AppearAnimation()
    {
        Rigidbody2D rb                  = GetComponent<Rigidbody2D>();
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        BoxCollider2D boxCollider       = GetComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer   = GetComponent<SpriteRenderer>();

        rb.bodyType            = RigidbodyType2D.Kinematic;
        circleCollider.enabled = false;
        boxCollider.enabled    = false;
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(APPEAR_DELAY);

        spriteRenderer.enabled = true;

        float timeElapsed  = 0f;
        Vector3 startPosition = transform.position;
        Vector3 endPosition   = transform.position + Vector3.up * APPEAR_HEIGHT;

        while (timeElapsed < APPEAR_DURATION)
        {
            float progress = timeElapsed / APPEAR_DURATION;
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        rb.bodyType            = RigidbodyType2D.Dynamic;
        circleCollider.enabled = true;
        boxCollider.enabled    = true;
    }
}