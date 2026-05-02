using System.Collections;
using UnityEngine;

public class DeathAnimation : MonoBehaviour
{
    public Sprite deadSprite;
    private SpriteRenderer spriteRenderer;

    private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

    private void OnEnable()
    { 
        spriteRenderer.enabled = true;
        spriteRenderer.sortingOrder = 20;  
        if (deadSprite != null) spriteRenderer.sprite = deadSprite;
 
        DisablePhysics();
         
        StartCoroutine(Animate());
    }

    private void DisablePhysics()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var c in colliders) c.enabled = false;

if (TryGetComponent(out Rigidbody2D rb))  
{
    rb.bodyType = RigidbodyType2D.Kinematic;
}        if (TryGetComponent(out PlayerMovement pm)) pm.enabled = false;
        if (TryGetComponent(out EntityMovement em)) em.enabled = false;
        if (TryGetComponent(out GoombaScript gs)) gs.enabled = false;
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        float duration = 3f;
        float jumpVelocity = 8f; 
        float gravity = -30f;    
        Vector3 velocity = Vector3.up * jumpVelocity;

        while (elapsed < duration)
        {
            transform.position += velocity * Time.deltaTime;
            velocity.y += gravity * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}


































