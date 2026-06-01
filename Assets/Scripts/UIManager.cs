using UnityEngine;
using TMPro;

 
public class UIManager : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    [Header("HUD Text")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI livesText;

    // ==========================================
    // 2. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        UpdateHUD();
    }

    // ==========================================
    // 3. PRIVATE METHODS
    // ==========================================
    private void UpdateHUD()
    {
        scoreText.text = "SCORE: " + GameManager.Instance.Score.ToString("D6");
        coinsText.text = "COINS: " + GameManager.Instance.Coins.ToString("D2");
        timeText.text  = "TIME: "  + GameManager.Instance.GameTime.ToString();
        livesText.text = "LIVES: "  + GameManager.Instance.Lives.ToString();
    }
}