using UnityEngine;

public class PowerUp : MonoBehaviour
{ 
    public enum Type
    {
        Coin,            
        ExtraLife,       
        MagicMushroom,   
        Starpower,      
    } 
    public Type type;

private void OnTriggerEnter2D(Collider2D other)
{
    
    if (other.CompareTag("Player")) 
    {
         
        if (other.TryGetComponent(out PlayerMovement player))
        {
            Collect(player);  
        }
    }
} 
    private void Collect(PlayerMovement  player)
    {
        if (TryGetComponent(out Collider2D col)) {
        col.enabled = false; 
    }
        switch (type)
        {
            case Type.Coin:
                GameManager.Instance.AddCoin();  
                break;

            case Type.ExtraLife:
                GameManager.Instance.AddLife();
                if (MusicManager.Instance != null) MusicManager.Instance.PlayLife();
                break;

            case Type.MagicMushroom:
                GameManager.Instance.AddScore(150);
                player.Grow();
                if (MusicManager.Instance != null) MusicManager.Instance.PlayGrow();
                break;

            case Type.Starpower:
                GameManager.Instance.AddScore(500);
                player.Starpower();
                break;
        }
 
        Destroy(gameObject);
    }
}