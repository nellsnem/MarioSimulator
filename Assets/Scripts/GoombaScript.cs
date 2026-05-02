using UnityEngine;

public class GoombaScript : MonoBehaviour
{
    public Sprite flatSprite;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!GetComponent<Collider2D>().enabled) return;
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
                if (player.IsStarpower)
                {
                    Hit();  
                    return;
                } 
            if (collision.transform.DotTest(transform, 0.5f)) {
                
                Flatten();
                GameManager.Instance.AddScore(300);
                
                Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f);
                return;  
            } 
             
            player.Hit();
        }
    }

    public void Flatten()
    {
        GetComponent<Collider2D>().enabled = false;
        if (TryGetComponent(out EntityMovement em)) em.enabled = false;
        if (TryGetComponent(out AnimatedSprite anim)) anim.enabled = false;
        GetComponent<SpriteRenderer>().sprite = flatSprite;
        Destroy(gameObject, 0.5f);
    }
 
    public void Hit()
    {   
        GameManager.Instance.AddScore(300);
        if (TryGetComponent(out DeathAnimation death)) {
            death.enabled = true; 
        } else {
            Destroy(gameObject);
        }
    }
}