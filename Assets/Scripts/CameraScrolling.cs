using UnityEngine;

public class CameraScrolling : MonoBehaviour
{
    public Transform player;

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 cameraPos = transform.position;
            
            // Камера рухається тільки вправо, якщо гравець пройшов далі її центру
            if (player.position.x > cameraPos.x) {
                cameraPos.x = player.position.x;
            }
            
            transform.position = cameraPos;
        }
    }
}