using UnityEngine;


public class PlayerVisuals : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Small Mario Sprites")]
    public Sprite smallIdle;
    public Sprite smallJump;
    public Sprite smallDeath;
    public Sprite[] smallRun;

    [Header("Big Mario Sprites")]
    public Sprite bigIdle;
    public Sprite bigJump;
    public Sprite[] bigRun;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private PlayerMovement _movement;
    private Rigidbody2D _rb;

    private int _runFrame;
    private float _frameTimer;
    private int _airFrames = 0;

    private int _colorIndex = 0;
    private float _colorTimer = 0f;

    // ==========================================
    // 3. CONSTANTS
    // ==========================================
    private const float COLOR_INTERVAL = 0.08f;
    private const float RUN_FRAME_INTERVAL = 0.1f;
    private const int AIR_FRAME_THRESHOLD = 3;

    private static readonly Color[] StarColors = new Color[]
    {
        Color.red, Color.yellow, Color.green,
        Color.cyan, Color.magenta, Color.white,
    };

    // ==========================================
    // 4. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _rb       = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (_movement.IsDead)
        {
            ShowDeathSprite();
            return;
        }

        UpdateStarpowerColor();
        AnimatePlayer();
    }

    // ==========================================
    // 5. PRIVATE METHODS
    // ==========================================
    private void ShowDeathSprite()
    {
        spriteRenderer.sprite = smallDeath;
        spriteRenderer.color  = Color.white;
    }

    private void UpdateStarpowerColor()
    {
        if (_movement.IsStarpower)
        {
            _colorTimer += Time.deltaTime;
            if (_colorTimer >= COLOR_INTERVAL)
            {
                _colorTimer  = 0f;
                _colorIndex  = (_colorIndex + 1) % StarColors.Length;
                spriteRenderer.color = StarColors[_colorIndex];
            }
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void AnimatePlayer()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Sprite currentIdle  = _movement.isBig ? bigIdle  : smallIdle;
        Sprite currentJump  = _movement.isBig ? bigJump  : smallJump;
        Sprite[] currentRun = _movement.isBig ? bigRun   : smallRun;
        Sprite currentSlide = (currentRun != null && currentRun.Length > 3) ? currentRun[3] : currentJump;

        UpdateAirFrames();

        bool isInAir   = !_movement.isGrounded && _airFrames > AIR_FRAME_THRESHOLD;
        bool isSliding = IsSliding();

        if (isInAir)
        {
            spriteRenderer.sprite = currentJump;
        }
        else if (isSliding)
        {
            spriteRenderer.sprite = currentSlide;
        }
        else if (Mathf.Abs(_rb.linearVelocity.x) > 0.1f)
        {
            AdvanceRunFrame(currentRun);
        }
        else
        {
            spriteRenderer.sprite = currentIdle;
            _runFrame = 0;
        }
    }

    private void UpdateAirFrames()
    {
        if (!_movement.isGrounded)
        {
            _airFrames++;
        }
        else
        {
            _airFrames = 0;
        }
    }

    private bool IsSliding()
    {
        return (_rb.linearVelocity.x > 0.1f  && Input.GetAxis("Horizontal") < 0) ||
               (_rb.linearVelocity.x < -0.1f && Input.GetAxis("Horizontal") > 0);
    }

    private void AdvanceRunFrame(Sprite[] currentRun)
    {
        _frameTimer += Time.deltaTime;
        if (_frameTimer >= RUN_FRAME_INTERVAL)
        {
            _frameTimer = 0;
            _runFrame   = (_runFrame + 1) % 3;

            if (currentRun != null && currentRun.Length > 0)
            {
                spriteRenderer.sprite = currentRun[_runFrame % currentRun.Length];
            }
        }
    }
}