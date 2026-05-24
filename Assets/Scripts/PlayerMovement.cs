using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using PinePie.SimpleJoystick;

public class PlayerMovement : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float coyoteTime = 0.2f;

    [Header("Status")]
    public bool isGrounded;
    public bool isBig;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private Rigidbody2D rb;
    private Camera mainCamera;
    private BoxCollider2D playerCollider;
    private JoystickController joystick;

    private bool isFacingRight = true;
    private bool isDead = false;
    private bool isInvincible = false;
    private bool isStarpower = false;
    private bool jumpPressed = false;
    private float coyoteCounter;

    // ==========================================
    // 3. PROPERTIES
    // ==========================================
    public bool IsDead => isDead;
    public bool IsStarpower => isStarpower;

    // ==========================================
    // 4. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        mainCamera = Camera.main;
        
        FindJoystickReference();
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        HandleCoyoteTime();
        HandleMovement();
        HandleJump();
    }

    private void LateUpdate()
    {
        if (isDead)
        {
            return;  
        }
 
        HandleCameraBounds();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        ResetGroundCollision(collision);
    }

    // ==========================================
    // 5. PUBLIC METHODS
    // ==========================================
    public void OnJumpButtonPressed()
    {
        jumpPressed = true;
    }

    public void Grow()
    {
        isBig = true;
        playerCollider.size = new Vector2(0.75f, 2f);
        playerCollider.offset = new Vector2(0f, 0.5f);
    }

    public void Starpower()
    {
        StartCoroutine(StarpowerRoutine());
    }

    public void Hit()
    {
        if (isInvincible || isDead)
        {
            return;
        }

        ProcessHitDamage();
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        ProcessDeathSequence();
    }

    // ==========================================
    // 6. PRIVATE METHODS
    // ==========================================
    private void FindJoystickReference()
    {
        joystick = FindAnyObjectByType<JoystickController>();
        
        if (joystick == null)
        {
            JoystickController[] allJoysticks = FindObjectsByType<JoystickController>(FindObjectsSortMode.None);
            foreach (var currentJoystick in allJoysticks) 
            {
                if (currentJoystick.name == "Joystick") 
                {
                    joystick = currentJoystick;
                    break;
                }
            }
        }
    }

    private void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        float moveInput = CalculateMoveInput();
        float targetSpeed = moveInput * moveSpeed;
        float acceleration = CalculateAcceleration(moveInput);

        rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.deltaTime),
            rb.linearVelocity.y
        );

        Vector3TurnCheck(moveInput);
    }

    private float CalculateMoveInput()
    {
        float joystickInput = (joystick != null) ? joystick.InputDirection.x : 0f;
        float keyboardInput = Input.GetAxis("Horizontal");
        
        return Mathf.Abs(joystickInput) > Mathf.Abs(keyboardInput) ? joystickInput : keyboardInput;
    }

    private float CalculateAcceleration(float moveInput)
    {
        if (moveInput != 0)
        {
            bool isTurning = (moveInput > 0 && rb.linearVelocity.x < -0.1f) ||
                             (moveInput < -0.01f && rb.linearVelocity.x > 0.1f);
            return isTurning ? moveSpeed * 2f : moveSpeed * 10f;
        }
        
        return moveSpeed * 15f;
    }

    private void Vector3TurnCheck(float moveInput)
    {
        if ((moveInput > 0 && !isFacingRight) || (moveInput < 0 && isFacingRight))
        {
            Flip();
        }
    }

    private void HandleJump()
    {
        bool jumpInput = Input.GetButtonDown("Jump") || jumpPressed;
        jumpPressed = false; 

        if (jumpInput && coyoteCounter > 0f)
        {
            ExecuteJump();
        }

        ApplyJumpModifiers();
    }

    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteCounter = 0f;
        isGrounded = false;
        
        if (MusicManager.Instance != null) 
        {
            MusicManager.Instance.PlayJump();
        }
    }

    private void ApplyJumpModifiers()
    {
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
    }

    private void HandleCameraBounds()
    {
        if (mainCamera == null) 
        {
            return;
        }

        Vector3 viewPos = transform.position;
        Vector3 leftEdge = mainCamera.ScreenToWorldPoint(Vector3.zero);
        float leftLimit = leftEdge.x;

        if (viewPos.x < leftLimit)
        {
            RestrictPlayerToLeft(viewPos, leftLimit);
        }
    }

    private void RestrictPlayerToLeft(Vector3 viewPos, float leftLimit)
    {
        viewPos.x = leftLimit;
        transform.position = viewPos;
 
        if (rb.linearVelocity.x < 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void Shrink()
    {
        isBig = false;
        playerCollider.size = new Vector2(0.75f, 1f);
        playerCollider.offset = new Vector2(0f, 0f);
    }

    private void ProcessHitDamage()
    {
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

    private void ProcessDeathSequence()
    {
        isDead = true;
        if (MusicManager.Instance != null) 
        {
            MusicManager.Instance.PlayDeath();
        }

        DisableColliders();
        DisableCameraScrolling();

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        StartCoroutine(DeathJump());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetLevel(3f);
        }
    }

    private void DisableColliders()
    {
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }
    }

    private void DisableCameraScrolling()
    {
        if (Camera.main != null && Camera.main.GetComponent<CameraScrolling>() != null)
        {
            Camera.main.GetComponent<CameraScrolling>().enabled = false;
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaleVector = transform.localScale;
        scaleVector.x *= -1;
        transform.localScale = scaleVector;
    }

    private void CheckGroundCollision(Collision2D collision)
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

    private void ResetGroundCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // ==========================================
    // 7. COROUTINES
    // ==========================================
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        SpriteRenderer spriteRendererComponent = GetComponent<PlayerVisuals>() != null
            ? GetComponent<PlayerVisuals>().spriteRenderer
            : GetComponent<SpriteRenderer>();

        float duration = 2f;
        float blinkInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            spriteRendererComponent.enabled = !spriteRendererComponent.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        spriteRendererComponent.enabled = true;
        isInvincible = false;
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
}