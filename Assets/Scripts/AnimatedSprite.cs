using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedSprite : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    [Header("Animation Settings")]
    public Sprite[] sprites;
    public float    framerate = 0.16f;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private SpriteRenderer _spriteRenderer;
    private int            _frame;

    // ==========================================
    // 3. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        StartAnimation();
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    // ==========================================
    // 4. PRIVATE METHODS
    // ==========================================
    private void StartAnimation()
    {
        InvokeRepeating(nameof(Animate), framerate, framerate);
    }

    private void StopAnimation()
    {
        CancelInvoke();
    }

    private void Animate()
    {
        _frame++;

        if (_frame >= sprites.Length)
        {
            _frame = 0;
        }

        if (_frame >= 0 && _frame < sprites.Length)
        {
            _spriteRenderer.sprite = sprites[_frame];
        }
    }
}