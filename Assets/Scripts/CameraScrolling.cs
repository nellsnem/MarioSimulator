using UnityEngine;

public class CameraScrolling : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    [Header("Targets")]
    public Transform player1;
    public Transform player2;

    [Header("Co-op Settings")]
    public float maxPlayerDistance = 14f;

    [Range(0f, 0.8f)]
    public float lookAheadFactor = 0.4f;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private float _halfScreenWidth;

    // ==========================================
    // 3. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Start()
    {
        InitScreenWidth();
    }

    private void LateUpdate()
    {
        FollowPlayers();
    }

    // ==========================================
    // 4. PRIVATE METHODS
    // ==========================================
    private void InitScreenWidth()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            _halfScreenWidth = cam.orthographicSize * cam.aspect;
        }
    }

    private void FollowPlayers()
    {
        if (player1 == null)
        {
            return;
        }

        Vector3 cameraPos = transform.position;
        float targetX     = cameraPos.x;

        bool isP2Active = player2 != null && player2.gameObject.activeInHierarchy;

        if (!isP2Active)
        {
            targetX = player1.position.x;
        }
        else
        {
            targetX = CalculateCoopTargetX();
        }

        if (targetX > cameraPos.x)
        {
            cameraPos.x = targetX;
        }

        transform.position = cameraPos;
    }

    private float CalculateCoopTargetX()
    {
        float p1X = player1.position.x;
        float p2X = player2.position.x;

        float laggingX  = Mathf.Min(p1X, p2X);
        float leadingX  = Mathf.Max(p1X, p2X);
        float lookahead = Mathf.Min(leadingX - laggingX, maxPlayerDistance) * lookAheadFactor;

        return laggingX + lookahead;
    }
}