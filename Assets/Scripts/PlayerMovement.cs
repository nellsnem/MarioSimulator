using UnityEngine;
using System.Collections;
using PinePie.SimpleJoystick;

public class PlayerMovement : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    [Header("Movement Settings")]
    public float moveSpeed  = 8f;
    public float jumpForce  = 14f;
    public float coyoteTime = 0.2f;

    [Header("Player Settings")]
    public int playerIndex = 1;

    [Header("Status")]
    public bool isGrounded;
    public bool isBig;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private Rigidbody2D        _rb;
    private Camera             _mainCamera;
    private Collider2D         _playerCollider;
    private JoystickController _joystick;

    private bool  _isFacingRight = true;
    private bool  _isDead        = false;
    private bool  _isInvincible  = false;
    private bool  _isStarpower   = false;
    private bool  _isJumpPressed = false;
    private float _coyoteCounter;

    // ==========================================
    // 3. CONSTANTS
    // ==========================================
    private const KeyCode MoveLeftKeyP1  = KeyCode.A;
    private const KeyCode MoveRightKeyP1 = KeyCode.D;
    private const KeyCode JumpKeyP1      = KeyCode.W;

    private const KeyCode MoveLeftKeyP2  = KeyCode.LeftArrow;
    private const KeyCode MoveRightKeyP2 = KeyCode.RightArrow;
    private const KeyCode JumpKeyP2      = KeyCode.UpArrow;

    private const float DefaultMoveSpeed  = 8f;
    private const float DefaultJumpForce  = 14f;
    private const float StarpowerSpeed    = 15f;
    private const float StarpowerJump     = 22f;
    private const float StarpowerDuration = 5f;

    // ==========================================
    // 4. PROPERTIES
    // ==========================================
    public bool IsDead      => _isDead;
    public bool IsStarpower => _isStarpower;

    // ==========================================
    // 5. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        _rb             = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponentInChildren<BoxCollider2D>();

        if (_playerCollider == null)
        {
            _playerCollider = GetComponentInChildren<CapsuleCollider2D>();
        }

        _mainCamera = Camera.main;
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
    // 6. PUBLIC METHODS
    // ==========================================
    public void OnJumpButtonPressed()
    {
        _isJumpPressed = true;
    }

    public void Grow()
    {
        if (_playerCollider == null)
        {
            return;
        }

        isBig = true;
        ApplyColliderSize(new Vector2(0.75f, 2f), new Vector2(0f, 0.5f));
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
        if (_isDead || _isInvincible)
        {
            return;
        }

        ProcessDeathSequence();
    }

    // ==========================================
    // 7. PRIVATE METHODS
    // ==========================================
    private void Shrink()
    {
        if (_playerCollider == null)
        {
            return;
        }

        isBig = false;
        ApplyColliderSize(new Vector2(0.75f, 1f), new Vector2(0f, 0f));
    }

    private void ApplyColliderSize(Vector2 size, Vector2 offset)
    {
        if (_playerCollider is BoxCollider2D box)
        {
            box.size   = size;
            box.offset = offset;
        }
        else if (_playerCollider is CapsuleCollider2D capsule)
        {
            capsule.size   = size;
            capsule.offset = offset;
        }
    }

    private void FindJoystickReference()
    {
        _joystick = FindAnyObjectByType<JoystickController>();

        if (_joystick != null)
        {
            return;
        }

        JoystickController[] allJoysticks = FindObjectsByType<JoystickController>(FindObjectsSortMode.None);

        foreach (JoystickController currentJoystick in allJoysticks)
        {
            if (currentJoystick.name == "Joystick")
            {
                _joystick = currentJoystick;
                return;
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
            _coyoteCounter -= UnityEngine.Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        float moveInput    = CalculateMoveInput();
        float targetSpeed  = moveInput * moveSpeed;
        float acceleration = CalculateAcceleration(moveInput);

        _rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, acceleration * UnityEngine.Time.deltaTime),
            _rb.linearVelocity.y
        );

        HandleTurnDirection(moveInput);
    }

    private float CalculateMoveInput()
    {
        float joystickInput = (_joystick != null) ? _joystick.InputDirection.x : 0f;
        float keyboardInput = GetHorizontalInput();

        return Mathf.Abs(joystickInput) > Mathf.Abs(keyboardInput) ? joystickInput : keyboardInput;
    }

    private float GetHorizontalInput()
    {
        return playerIndex == 2
            ? GetRawAxis(MoveLeftKeyP2, MoveRightKeyP2)
            : GetRawAxis(MoveLeftKeyP1, MoveRightKeyP1);
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
        if (moveInput == 0)
        {
            return moveSpeed * 15f;
        }

        bool isTurning = (moveInput > 0    && _rb.linearVelocity.x < -0.1f)
                      || (moveInput < -0.01f && _rb.linearVelocity.x > 0.1f);

        return isTurning ? moveSpeed * 2f : moveSpeed * 10f;
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
        return playerIndex == 2
            ? Input.GetKeyDown(JumpKeyP2)
            : Input.GetKeyDown(JumpKeyP1) || Input.GetKeyDown(KeyCode.Space);
    }

    private void ExecuteJump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        _coyoteCounter     = 0f;
        isGrounded         = false;

        MusicManager.Instance?.PlayJump();
    }

    private void ApplyJumpModifiers()
    {
        if (isGrounded)
        {
            return;
        }

        bool isFalling      = _rb.linearVelocity.y < 0;
        bool isRisingNoHold = _rb.linearVelocity.y > 0 && !IsJumpHeld();

        if (isFalling)
        {
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 2f * UnityEngine.Time.deltaTime;
        }
        else if (isRisingNoHold)
        {
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 3f * UnityEngine.Time.deltaTime;
        }
    }

    private bool IsJumpHeld()
    {
        return playerIndex == 2
            ? Input.GetKey(JumpKeyP2)
            : Input.GetKey(JumpKeyP1) || Input.GetKey(KeyCode.Space);
    }

    private void HandleCameraBounds()
    {
        if (_mainCamera == null)
        {
            return;
        }

        Vector3 pos       = transform.position;
        Vector3 leftEdge  = _mainCamera.ScreenToWorldPoint(Vector3.zero);
        Vector3 rightEdge = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));

        if (pos.x < leftEdge.x)
        {
            RestrictPlayerToLeft(pos, leftEdge.x);
        }

        if (pos.x > rightEdge.x)
        {
            ClampPlayerToRight(pos, rightEdge.x);
        }
    }

    private void RestrictPlayerToLeft(Vector3 currentPos, float leftLimit)
    {
        currentPos.x       = leftLimit;
        transform.position = currentPos;

        if (_rb.linearVelocity.x < 0)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        }
    }

    private void ClampPlayerToRight(Vector3 currentPos, float rightLimit)
    {
        currentPos.x       = rightLimit;
        transform.position = currentPos;

        if (_rb.linearVelocity.x > 0)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        }
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

        MusicManager.Instance?.PlayDeath();
        DisableColliders();
        DisableCameraScrolling();

        _rb.gravityScale   = 0f;
        _rb.linearVelocity = Vector2.zero;

        StartCoroutine(DeathJump());
        GameManager.Instance?.ResetLevel(3f);
        MakeOtherPlayersInvincible(3f);
    }

    private void MakeOtherPlayersInvincible(float duration)
    {
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (PlayerMovement other in allPlayers)
        {
            if (other != this && !other.IsDead)
            {
                other.StartCoroutine(other.InvincibilityFrames(duration));
            }
        }
    }

    private void DisableCameraScrolling()
    {
        CameraScrolling cam = Camera.main != null
            ? Camera.main.GetComponent<CameraScrolling>()
            : null;

        if (cam == null)
        {
            return;
        }

        if (playerIndex == 1)
		{
			cam.player1 = null;
		} 
        if (playerIndex == 2)
		{
			cam.player2 = null;
		}

        if (cam.player1 == null && cam.player2 == null)
        {
            cam.enabled = false;
        }
    }

    private void DisableColliders()
    {
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }
    }

    private void Flip()
    {
        _isFacingRight       = !_isFacingRight;
        Vector3 scaleVector  = transform.localScale;
        scaleVector.x       *= -1;
        transform.localScale = scaleVector;
    }

    private void CheckGroundCollision(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
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

    private SpriteRenderer ResolvePlayerSpriteRenderer()
    {
        PlayerVisuals visuals = GetComponent<PlayerVisuals>();

        if (visuals != null && visuals.spriteRenderer != null)
        {
            return visuals.spriteRenderer;
        }

        return GetComponentInChildren<SpriteRenderer>();
    }

    // ==========================================
    // 8. COROUTINES
    // ==========================================
    public IEnumerator InvincibilityFrames(float duration = 2f)
    {
        _isInvincible = true;

        SpriteRenderer spriteRendererComponent = ResolvePlayerSpriteRenderer();

        if (spriteRendererComponent == null)
        {
            _isInvincible = false;
            yield break;
        }

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

        moveSpeed = StarpowerSpeed;
        jumpForce = StarpowerJump;

        yield return new WaitForSeconds(StarpowerDuration);

        moveSpeed = DefaultMoveSpeed;
        jumpForce = DefaultJumpForce;

        _isInvincible = false;
        _isStarpower  = false;
    }
}