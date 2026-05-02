using System.Collections;
using UnityEngine;

public class BlockItem : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(AppearAnimation());
    }

    private IEnumerator AppearAnimation()
    { 
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
 
        rb.bodyType = RigidbodyType2D.Kinematic;
        circleCollider.enabled = false;
        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
 
        yield return new WaitForSeconds(0.25f);
 
        spriteRenderer.enabled = true;

        float timeElapsed = 0f;
        float animationDuration = 0.5f;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + Vector3.up;  

        while (timeElapsed < animationDuration)
        {
            float progress = timeElapsed / animationDuration;
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);

            timeElapsed += Time.deltaTime;
            yield return null;  
        }
 
        rb.bodyType = RigidbodyType2D.Dynamic;

        circleCollider.enabled = true;
        boxCollider.enabled = true;
    }
}