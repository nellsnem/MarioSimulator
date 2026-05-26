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

    [Header("Player Settings")]
    public int playerIndex = 1;

    [Header("Status")]
    public bool isGrounded;
    public bool isBig;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private Rigidbody2D _rb;
    private Camera _mainCamera;
    private BoxCollider2D _playerCollider;
    private JoystickController _joystick;

    private bool _isFacingRight = true;
    private bool _isDead = false;
    private bool _isInvincible = false;
    private bool _isStarpower = false;
    private bool _isJumpPressed = false;
    private float _coyoteCounter;

    // Player 1 keys
    private const KeyCode MoveLeftKeyP1  = KeyCode.A;
    private const KeyCode MoveRightKeyP1 = KeyCode.D;
    private const KeyCode JumpKeyP1      = KeyCode.W;

    // Player 2 keys
    private const KeyCode MoveLeftKeyP2  = KeyCode.LeftArrow;
    private const KeyCode MoveRightKeyP2 = KeyCode.RightArrow;
    private const KeyCode JumpKeyP2      = KeyCode.UpArrow;

    // ==========================================
    // 3. PROPERTIES
    // ==========================================
    public bool IsDead      => _isDead;
    public bool IsStarpower => _isStarpower;

    // ==========================================
    // 4. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        _rb             = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<BoxCollider2D>();
        _mainCamera     = Camera.main;

        FindJoystickReference();
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }

        HandleCoyoteTime();
        HandleMovement();
        HandleJump();
    }

    private void LateUpdate()
    {
        if (_isDead)
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
        _isJumpPressed = true;
    }

    public void Grow()
    {
        isBig                   = true;
        _playerCollider.size   = new Vector2(0.75f, 2f);
        _playerCollider.offset = new Vector2(0f, 0.5f);
    }

    public void Starpower()
    {
        StartCoroutine(StarpowerRoutine());
    }

    public void Hit()
    {
        if (_isInvincible || _isDead)
        {
            return;
        }

        ProcessHitDamage();
    }

    public void Die()
    {
        if (_isDead)
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
        _joystick = FindAnyObjectByType<JoystickController>();

        if (_joystick == null)
        {
            JoystickController[] allJoysticks = FindObjectsByType<JoystickController>(FindObjectsSortMode.None);
            foreach (var currentJoystick in allJoysticks)
            {
                if (currentJoystick.name == "Joystick")
                {
                    _joystick = currentJoystick;
                    break;
                }
            }
        }
    }

    private void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            _coyoteCounter = coyoteTime;
        }
        else
        {
            _coyoteCounter -= Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        float moveInput   = CalculateMoveInput();
        float targetSpeed = moveInput * moveSpeed;
        float acceleration = CalculateAcceleration(moveInput);

        _rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, acceleration * Time.deltaTime),
            _rb.linearVelocity.y
        );

        HandleTurnDirection(moveInput);
    }

    private float CalculateMoveInput()
    {
        float joystickInput  = (_joystick != null) ? _joystick.InputDirection.x : 0f;
        float keyboardInput  = GetHorizontalInput();

        return Mathf.Abs(joystickInput) > Mathf.Abs(keyboardInput) ? joystickInput : keyboardInput;
    }

    private float GetHorizontalInput()
    {
        if (playerIndex == 2)
        {
            return GetRawAxis(MoveLeftKeyP2, MoveRightKeyP2);
        }

        return GetRawAxis(MoveLeftKeyP1, MoveRightKeyP1);
    }

    private float GetRawAxis(KeyCode leftKey, KeyCode rightKey)
    {
        if (Input.GetKey(leftKey))
        {
            return -1f;
        }

        if (Input.GetKey(rightKey))
        {
            return 1f;
        }

        return 0f;
    }

    private float CalculateAcceleration(float moveInput)
    {
        if (moveInput != 0)
        {
            bool isTurning = (moveInput > 0 && _rb.linearVelocity.x < -0.1f) ||
                             (moveInput < -0.01f && _rb.linearVelocity.x > 0.1f);
            return isTurning ? moveSpeed * 2f : moveSpeed * 10f;
        }

        return moveSpeed * 15f;
    }

    private void HandleTurnDirection(float moveInput)
    {
        if ((moveInput > 0 && !_isFacingRight) || (moveInput < 0 && _isFacingRight))
        {
            Flip();
        }
    }

    private void HandleJump()
    {
        bool jumpInput = GetJumpInput() || _isJumpPressed;
        _isJumpPressed = false;

        if (jumpInput && _coyoteCounter > 0f)
        {
            ExecuteJump();
        }

        ApplyJumpModifiers();
    }

    private bool GetJumpInput()
    {
        if (playerIndex == 2)
        {
            return Input.GetKeyDown(JumpKeyP2);
        }

        return Input.GetKeyDown(JumpKeyP1);
    }

    private void ExecuteJump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        _coyoteCounter     = 0f;
        isGrounded         = false;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayJump();
        }
    }

    private void ApplyJumpModifiers()
    {
        if (!isGrounded)
        {
            bool isFalling      = _rb.linearVelocity.y < 0;
            bool isRisingNoHold = _rb.linearVelocity.y > 0 && !IsJumpHeld();

            if (isFalling)
            {
                _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 2f * Time.deltaTime;
            }
            else if (isRisingNoHold)
            {
                _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 3f * Time.deltaTime;
            }
        }
    }

    private bool IsJumpHeld()
    {
        if (playerIndex == 2)
        {
            return Input.GetKey(JumpKeyP2);
        }

        return Input.GetKey(JumpKeyP1);
    }

    private void HandleCameraBounds()
    {
        if (_mainCamera == null)
        {
            return;
        }

        Vector3 viewPos  = transform.position;
        Vector3 leftEdge = _mainCamera.ScreenToWorldPoint(Vector3.zero);
        float leftLimit  = leftEdge.x;

        if (viewPos.x < leftLimit)
        {
            RestrictPlayerToLeft(viewPos, leftLimit);
        }
    }

    private void RestrictPlayerToLeft(Vector3 viewPos, float leftLimit)
    {
        viewPos.x         = leftLimit;
        transform.position = viewPos;

        if (_rb.linearVelocity.x < 0)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        }
    }

    private void Shrink()
    {
        isBig                   = false;
        _playerCollider.size   = new Vector2(0.75f, 1f);
        _playerCollider.offset = new Vector2(0f, 0f);
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
        _isDead = true;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayDeath();
        }

        DisableColliders();
        DisableCameraScrolling();

        _rb.gravityScale   = 0f;
        _rb.linearVelocity = Vector2.zero;

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
        CameraScrolling cameraScrolling = Camera.main != null
            ? Camera.main.GetComponent<CameraScrolling>()
            : null;

        if (cameraScrolling != null)
        {
            cameraScrolling.enabled = false;
        }
    }

    private void Flip()
    {
        _isFacingRight    = !_isFacingRight;
        Vector3 scaleVector = transform.localScale;
        scaleVector.x      *= -1;
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
        _isInvincible = true;

        SpriteRenderer spriteRendererComponent = GetComponent<PlayerVisuals>() != null
            ? GetComponent<PlayerVisuals>().spriteRenderer
            : GetComponent<SpriteRenderer>();

        float duration      = 2f;
        float blinkInterval = 0.1f;
        float elapsed       = 0f;

        while (elapsed < duration)
        {
            spriteRendererComponent.enabled = !spriteRendererComponent.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        spriteRendererComponent.enabled = true;
        _isInvincible = false;
    }

    private IEnumerator DeathJump()
    {
        yield return null;
        _rb.gravityScale   = 1f;
        _rb.linearVelocity = new Vector2(0f, 6f);
        yield return new WaitUntil(() => _rb.linearVelocity.y <= 0f);
        _rb.bodyType       = RigidbodyType2D.Kinematic;
        _rb.linearVelocity = new Vector2(0f, -8f);
    }

    private IEnumerator StarpowerRoutine()
    {
        _isStarpower  = true;
        _isInvincible = true;

        moveSpeed  = 15f;
        jumpForce  = 22f;

        yield return new WaitForSeconds(5f);

        moveSpeed  = 8f;
        jumpForce  = 20f;

        _isInvincible = false;
        _isStarpower  = false;
    }
}