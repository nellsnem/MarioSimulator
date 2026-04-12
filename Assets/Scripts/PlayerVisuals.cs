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
    private float airTime;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (movement.IsDead) {
            spriteRenderer.sprite = smallDeath;
            return;
        }

        AnimatePlayer();
    }

    private void AnimatePlayer()
    {
        if (spriteRenderer == null) return;

        Sprite currentIdle = movement.isBig ? bigIdle : smallIdle;
        Sprite currentJump = movement.isBig ? bigJump : smallJump;
        Sprite[] currentRun = movement.isBig ? bigRun : smallRun;
        Sprite currentSlide = (currentRun.Length > 3) ? currentRun[3] : currentJump;

        if (!movement.isGrounded) airTime += Time.deltaTime;
        else airTime = 0;

        bool isSliding = (rb.linearVelocity.x > 0.1f && Input.GetAxis("Horizontal") < 0) || 
                         (rb.linearVelocity.x < -0.1f && Input.GetAxis("Horizontal") > 0);

        if (!movement.isGrounded && airTime > 0.1f) {
            spriteRenderer.sprite = currentJump;
        } else if (isSliding) {
            spriteRenderer.sprite = currentSlide;
        } else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f) {
            frameTimer += Time.deltaTime;
            if (frameTimer >= 0.1f) {
                frameTimer = 0;
                runFrame++;
                if (runFrame >= 3) runFrame = 0; 
                spriteRenderer.sprite = currentRun[runFrame];
            }
        } else {
            spriteRenderer.sprite = currentIdle;
        }
    }
}