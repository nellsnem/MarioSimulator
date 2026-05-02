using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    private PlayerMovement movement;
    private Rigidbody2D rb;

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

    private int runFrame;
    private float frameTimer;
 
    private int airFrames = 0;

    private Color[] starColors = new Color[]
    {
        Color.red, Color.yellow, Color.green,
        Color.cyan, Color.magenta, Color.white,
    };
    private int colorIndex = 0;
    private float colorTimer = 0f;
    private float colorInterval = 0.08f;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (movement.IsDead)
        {
            spriteRenderer.sprite = smallDeath;
            spriteRenderer.color = Color.white;
            return;
        }
 
        if (movement.IsStarpower)
        {
            colorTimer += Time.deltaTime;
            if (colorTimer >= colorInterval)
            {
                colorTimer = 0f;
                colorIndex = (colorIndex + 1) % starColors.Length;
                spriteRenderer.color = starColors[colorIndex];
            }
        }
        else
        {
            spriteRenderer.color = Color.white;
        }

        AnimatePlayer();
    }

    private void AnimatePlayer()
    {
        if (spriteRenderer == null) return;

        Sprite currentIdle = movement.isBig ? bigIdle : smallIdle;
        Sprite currentJump = movement.isBig ? bigJump : smallJump;
        Sprite[] currentRun = movement.isBig ? bigRun : smallRun;
        Sprite currentSlide = (currentRun != null && currentRun.Length > 3) ? currentRun[3] : currentJump;
 
        if (!movement.isGrounded)
        {
            airFrames++;
        }
        else
        {
            airFrames = 0;
        }
 
        bool isInAir = !movement.isGrounded && airFrames > 3;

        bool isSliding = (rb.linearVelocity.x > 0.1f && Input.GetAxis("Horizontal") < 0) ||
                         (rb.linearVelocity.x < -0.1f && Input.GetAxis("Horizontal") > 0);

        if (isInAir)
        {
            spriteRenderer.sprite = currentJump;
        }
        else if (isSliding)
        {
            spriteRenderer.sprite = currentSlide;
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= 0.1f)
            {
                frameTimer = 0;
                runFrame = (runFrame + 1) % 3;

                if (currentRun != null && currentRun.Length > 0)
                    spriteRenderer.sprite = currentRun[runFrame % currentRun.Length];
            }
        }
        else
        {
            spriteRenderer.sprite = currentIdle;
            runFrame = 0;  
        }
    }
}