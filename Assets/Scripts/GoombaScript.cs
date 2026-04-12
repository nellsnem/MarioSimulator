using UnityEngine;

public class GoombaScript : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite flatSprite;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();

            if (collision.transform.DotTest(transform, 0.5f)) {
                Flatten();
                
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null) {
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 10f);
                }
            } else {
                player.Die(); 
            }
        }
    }

    private void Flatten()
    {
        GetComponent<Collider2D>().enabled = false;
        
        if (GetComponent<EntityMovement>() != null) {
            GetComponent<EntityMovement>().enabled = false;
        }
        
        if (GetComponent<AnimatedSprite>() != null) {
            GetComponent<AnimatedSprite>().enabled = false;
        }

        GetComponent<SpriteRenderer>().sprite = flatSprite;
        Destroy(gameObject, 0.5f);
    }
}