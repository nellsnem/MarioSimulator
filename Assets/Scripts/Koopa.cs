using UnityEngine;
using System.Collections;


public class Koopa : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    public Sprite shellSprite;
    public float shellSpeed = 12f;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private bool _inShell;
    private bool _isPushed;
    private bool _bounceCooldown = false;
    private EntityMovement _movement;

    // ==========================================
    // 3. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        _movement = GetComponent<EntityMovement>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
            return;
        }

        if (_isPushed && !collision.gameObject.CompareTag("Player") &&
            !collision.gameObject.CompareTag("Enemy") && !_bounceCooldown)
        {
            HandleWallBounce(collision);
        }

        if (_isPushed && collision.gameObject.CompareTag("Enemy"))
        {
            HandleEnemyCollision(collision);
        }
    }

    // ==========================================
    // 4. PUBLIC METHODS
    // ==========================================
    public void Hit()
    {
        if (TryGetComponent(out DeathAnimation death))
        {
            death.enabled = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // 5. PRIVATE METHODS
    // ==========================================
    private void HandlePlayerCollision(Collision2D collision)
    {
        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();

        if (player.IsStarpower)
        {
            Hit();
            return;
        }

        if (!_inShell && collision.transform.DotTest(transform, 0.5f))
        {
            EnterShell();
            BouncePlayer(collision.gameObject);
            return;
        }

        if (_inShell && !_isPushed)
        {
            PushShellFromPlayer(collision);
            return;
        }

        if (!_inShell || _isPushed)
        {
            if (player != null)
            {
                player.Hit();
            }
        }
    }

    private void PushShellFromPlayer(Collision2D collision)
    {
        Collider2D shellCollider  = GetComponent<Collider2D>();
        Collider2D playerCollider = collision.collider;

        Physics2D.IgnoreCollision(shellCollider, playerCollider, true);
        StartCoroutine(RestoreCollision(shellCollider, playerCollider, 0.3f));

        float dirX = transform.position.x - collision.transform.position.x;
        Push(new Vector2(dirX, 0f));
    }

    private void HandleWallBounce(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                _movement.direction = new Vector2(-Mathf.Sign(_movement.direction.x), 0f);
                transform.position += (Vector3)(contact.normal * 0.15f);
                StartCoroutine(BounceCooldown());
                return;
            }
        }
    }

    private void HandleEnemyCollision(Collision2D collision)
    {
        if (collision.gameObject == gameObject)
        {
            return;
        }

        GameManager.Instance.AddScore(500);
        collision.gameObject.SendMessage("Hit", SendMessageOptions.DontRequireReceiver);
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
    }

    private void EnterShell()
    {
        _inShell = true;
        GetComponent<SpriteRenderer>().sprite = shellSprite;

        if (TryGetComponent(out AnimatedSprite anim))
        {
            anim.enabled = false;
        }

        _movement.enabled = false;
    }

    private void Push(Vector2 direction)
    {
        _isPushed            = true;
        _movement.speed      = shellSpeed;
        _movement.direction  = direction.normalized;
        _movement.enabled    = true;
        gameObject.layer     = LayerMask.NameToLayer("Shell");
    }

    private void BouncePlayer(GameObject playerObject)
    {
        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f);
        }
    }

    private void OnBecameInvisible()
    {
        if (_isPushed)
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // 6. COROUTINES
    // ==========================================
    private IEnumerator BounceCooldown()
    {
        _bounceCooldown = true;
        yield return new WaitForSeconds(0.1f);
        _bounceCooldown = false;
    }

    private IEnumerator RestoreCollision(Collider2D a, Collider2D b, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (a != null && b != null)
        {
            Physics2D.IgnoreCollision(a, b, false);
        }
    }
}