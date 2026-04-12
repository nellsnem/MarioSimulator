using UnityEngine;
using UnityEngine.SceneManagement;

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

    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (isDead) return;

        float moveInput = Input.GetAxis("Horizontal");
        float targetSpeed = moveInput * moveSpeed;

        if (isGrounded) {
            coyoteCounter = coyoteTime;
        } else {
            coyoteCounter -= Time.deltaTime;
        }

        float acceleration;
        if (moveInput != 0) {
            bool isTurning = (moveInput > 0 && rb.linearVelocity.x < -0.1f) || (moveInput < -0.01f && rb.linearVelocity.x > 0.1f);
            acceleration = isTurning ? moveSpeed * 2f : moveSpeed * 10f;
        } else {
            acceleration = moveSpeed * 15f;
        }

        rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.deltaTime), rb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && coyoteCounter > 0f) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteCounter = 0f;
            isGrounded = false;
        }

        if (!isGrounded) {
            if (rb.linearVelocity.y < 0) {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 2f * Time.deltaTime;
            } else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump")) {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 3f * Time.deltaTime;
            }
        }

        if ((moveInput > 0 && !isFacingRight) || (moveInput < 0 && isFacingRight)) Flip();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) {
            if (isBig) Shrink(); else Die();
        }
    }

    private void Shrink() { isBig = false; }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        GetComponent<Collider2D>().enabled = false;
        
        if (Camera.main.GetComponent<CameraScrolling>() != null) {
            Camera.main.GetComponent<CameraScrolling>().enabled = false;
        }

        rb.linearVelocity = new Vector2(0, 12f);
        
        if (GameManager.Instance != null) {
            GameManager.Instance.ResetLevel(3f);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) {
            foreach (ContactPoint2D contact in collision.contacts) {
                if (contact.normal.y > 0.5f) { 
                    isGrounded = true; 
                    return; 
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision) 
    { 
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false; 
    }

    private void LateUpdate()
    {
        Vector3 viewPos = transform.position;
        Vector3 leftEdge = mainCamera.ScreenToWorldPoint(Vector3.zero);
        if (viewPos.x < leftEdge.x + 0.5f) viewPos.x = leftEdge.x + 0.5f;
        transform.position = viewPos;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }
}