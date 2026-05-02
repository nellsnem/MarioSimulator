using UnityEngine;
using TMPro;  

public class UIManager : MonoBehaviour
{
    [Header("Налаштування тексту")]
    public TextMeshProUGUI scoreText;    
    public TextMeshProUGUI coinsText;   
    public TextMeshProUGUI timeText;    
    public TextMeshProUGUI livesText; 

    private void Update()
    {
         
        if (GameManager.Instance != null) 
        {
             
            scoreText.text = "SCORE: " +GameManager.Instance.score.ToString("D6");
            
           
            coinsText.text = "COINS: " + GameManager.Instance.coins.ToString("D2"); 
            
            
            timeText.text = "TIME: " + GameManager.Instance.time.ToString();
            
          
            livesText.text = "LIVES:  " + GameManager.Instance.lives.ToString();
        }
    }
}