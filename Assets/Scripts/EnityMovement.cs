using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMovement : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    [Header("Movement Settings")]
    public float   speed     = 1f;
    public Vector2 direction = Vector2.left;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private Rigidbody2D _rb;
    private Vector2     _velocity;

    // ==========================================
    // 3. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        _rb     = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    private void OnEnable()
    {
        WakeUpBody();
    }

    private void OnDisable()
    {
        SleepBody();
    }

    private void OnBecameVisible()
    {
        enabled = true;
    }

    private void OnBecameInvisible()
    {
        enabled = false;
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ClampGroundVelocity();
        CheckWallCollision();
    }

    // ==========================================
    // 4. PRIVATE METHODS
    // ==========================================
    private void WakeUpBody()
    {
        _rb.WakeUp();
    }

    private void SleepBody()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.Sleep();
    }

    private void ApplyMovement()
    {
        _velocity.x  = direction.x * speed;
        _velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;

        _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
    }

    private void ClampGroundVelocity()
    {
        if (_rb.Raycast(Vector2.down))
        {
            _velocity.y = Mathf.Max(_velocity.y, 0f);
        }
    }

    private void CheckWallCollision()
    {
        if (_rb.Raycast(direction))
        {
            direction = -direction;
        }
    }
}