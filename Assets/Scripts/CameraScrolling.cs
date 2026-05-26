using UnityEngine;
 
public class CameraScrolling : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    public Transform player;

    // ==========================================
    // 2. MONOBEHAVIOUR METHODS
    // ==========================================
    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        FollowPlayer();
    }

    // ==========================================
    // 3. PRIVATE METHODS
    // ==========================================
    private void FollowPlayer()
    {
        Vector3 cameraPos = transform.position;

        if (player.position.x > cameraPos.x)
        {
            cameraPos.x = player.position.x;
        }

        transform.position = cameraPos;
    }
}