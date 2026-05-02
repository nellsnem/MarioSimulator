using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using PinePie.SimpleJoystick;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera mainCamera;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float coyoteTime = 0.2f;

    [Header("Status")]
    public bool isGrounded;
    public bool isBig;
    private bool isFacingRight = true;
    private bool isDead = false;
    private float coyoteCounter;

    private bool isInvincible = false;
    public bool IsDead => isDead;

    private BoxCollider2D playerCollider;


    private JoystickController joystick;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        joystick = FindAnyObjectByType<JoystickController>();
        
        if (joystick == null)
        {
             JoystickController[] allJoysticks = FindObjectsByType<JoystickController>(FindObjectsSortMode.None);
             foreach (var j in allJoysticks) {
                 if (j.name == "Joystick") {
                     joystick = j;
                     break;
                 }
             }
        }
        if (isDead) return;

        float joystickInput = (joystick != null) ? joystick.InputDirection.x : 0f;
        float keyboardInput = Input.GetAxis("Horizontal");
        float moveInput = Mathf.Abs(joystickInput) > Mathf.Abs(keyboardInput) ? joystickInput : keyboardInput;

        float targetSpeed = moveInput * moveSpeed;

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        float acceleration;
        if (moveInput != 0)
        {
            bool isTurning = (moveInput > 0 && rb.linearVelocity.x < -0.1f) ||
                             (moveInput < -0.01f && rb.linearVelocity.x > 0.1f);
            acceleration = isTurning ? moveSpeed * 2f : moveSpeed * 10f;
        }
        else
        {
            acceleration = moveSpeed * 15f;
        }

        rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.deltaTime),
            rb.linearVelocity.y
        );

        // Стрибок: кнопка на екрані (jumpPressed) АБО клавіатура
        bool jumpInput = Input.GetButtonDown("Jump") || jumpPressed;
        jumpPressed = false; // скидаємо після використання

        if (jumpInput && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteCounter = 0f;
            isGrounded = false;
            if (MusicManager.Instance != null) MusicManager.Instance.PlayJump();
        }

        if (!isGrounded)
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 2f * Time.deltaTime;
            }
            else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 3f * Time.deltaTime;
            }
        }

        if ((moveInput > 0 && !isFacingRight) || (moveInput < 0 && isFacingRight))
            Flip();
    }

    // Викликається кнопкою стрибка на екрані (прив'яжи у Inspector)
    private bool jumpPressed = false;
    public void OnJumpButtonPressed() => jumpPressed = true;

    private void LateUpdate()
    {
        if (isDead) return;  
 
        if (mainCamera == null) return;

        Vector3 viewPos = transform.position;
        Vector3 leftEdge = mainCamera.ScreenToWorldPoint(Vector3.zero);

       
        float leftLimit = leftEdge.x;
        if (viewPos.x < leftLimit)
        {
            viewPos.x = leftLimit;
            transform.position = viewPos;
 
            if (rb.linearVelocity.x < 0)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void Shrink()
    {
        isBig = false;
        playerCollider.size = new Vector2(0.75f, 1f);
        playerCollider.offset = new Vector2(0f, 0f);
    }

    private IEnumerator InvincibilityFrames()
    {
        
        isInvincible = true;

        SpriteRenderer sr = GetComponent<PlayerVisuals>() != null
            ? GetComponent<PlayerVisuals>().spriteRenderer
            : GetComponent<SpriteRenderer>();

        float duration = 2f;
        float blinkInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        sr.enabled = true;
        isInvincible = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        if (MusicManager.Instance != null) MusicManager.Instance.PlayDeath();

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        if (Camera.main != null && Camera.main.GetComponent<CameraScrolling>() != null)
            Camera.main.GetComponent<CameraScrolling>().enabled = false;

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        StartCoroutine(DeathJump());

        if (GameManager.Instance != null)
            GameManager.Instance.ResetLevel(3f);
    }

    private IEnumerator DeathJump()
    {
        yield return null;
        rb.gravityScale = 1f;
        rb.linearVelocity = new Vector2(0f, 6f);
        yield return new WaitUntil(() => rb.linearVelocity.y <= 0f);
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = new Vector2(0f, -8f);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    return;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    public void Grow()
    {
        isBig = true;
        playerCollider.size = new Vector2(0.75f, 2f);
        playerCollider.offset = new Vector2(0f, 0.5f);
    }

    private bool isStarpower = false;
    public bool IsStarpower => isStarpower;

    public void Starpower()
    {
        StartCoroutine(StarpowerRoutine());
    }

   private IEnumerator StarpowerRoutine()
{
    isStarpower = true; 
    isInvincible = true; 
 
    moveSpeed = 15f; 
    jumpForce = 22f; 

    yield return new WaitForSeconds(5f); 
 
    moveSpeed = 8f; 
    jumpForce = 20f; 

    isInvincible = false; 
    isStarpower = false; 
}

    public void Hit()
    {
        if (isInvincible || isDead) return;
        if (isBig)
        {
            Shrink();
            StartCoroutine(InvincibilityFrames());
        }
        else
        {
            Die();
        }
    }
}