using UnityEngine;
using System.Collections;

public class Koopa : MonoBehaviour
{
    public Sprite shellSprite;
    public float shellSpeed = 12f;
    private bool inShell;
    private bool isPushed;
    private EntityMovement movement;

    private bool bounceCooldown = false;

    private void Awake() => movement = GetComponent<EntityMovement>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
 
            if (player.IsStarpower)
            {
                Hit();
                return;
            }
 
            if (!inShell && collision.transform.DotTest(transform, 0.5f))
            {
                EnterShell();
                BouncePlayer(collision.gameObject);
                return;
            }
 
            if (inShell && !isPushed)
            {
                Collider2D shellCollider = GetComponent<Collider2D>();
                Collider2D playerCollider = collision.collider;
                Physics2D.IgnoreCollision(shellCollider, playerCollider, true);
                StartCoroutine(RestoreCollision(shellCollider, playerCollider, 0.3f));

                float dirX = transform.position.x - collision.transform.position.x;
                Push(new Vector2(dirX, 0f)); 
                return;
            }
 
            if (!inShell || isPushed)
            {
                if (player != null) player.Hit();
            }
 
        }
 
        if (isPushed
            && !collision.gameObject.CompareTag("Player")
            && !collision.gameObject.CompareTag("Enemy")
            && !bounceCooldown)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    movement.direction = new Vector2(-Mathf.Sign(movement.direction.x), 0f);
                    transform.position += (Vector3)(contact.normal * 0.15f);
                    StartCoroutine(BounceCooldown());
                    return;
                }
            }
        }
 
        if (isPushed && collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject != gameObject)
            {
                GameManager.Instance.AddScore(500);
                collision.gameObject.SendMessage("Hit", SendMessageOptions.DontRequireReceiver);
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
            }
        }
    }

    private IEnumerator BounceCooldown()
    {
        bounceCooldown = true;
        yield return new WaitForSeconds(0.1f);
        bounceCooldown = false;
    }

    private IEnumerator RestoreCollision(Collider2D a, Collider2D b, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (a != null && b != null)
            Physics2D.IgnoreCollision(a, b, false);
    }

    private void EnterShell()
    {
        inShell = true;
        GetComponent<SpriteRenderer>().sprite = shellSprite;
        if (TryGetComponent(out AnimatedSprite anim)) anim.enabled = false;
        movement.enabled = false;
    }

    private void Push(Vector2 direction)
    {
        isPushed = true;
        movement.speed = shellSpeed;
        movement.direction = direction.normalized;
        movement.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Shell");
    }

    private void BouncePlayer(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f);
    }

    public void Hit()
    {
        if (TryGetComponent(out DeathAnimation death)) death.enabled = true;
        else Destroy(gameObject);
    }

    private void OnBecameInvisible() { if (isPushed) Destroy(gameObject); }
}