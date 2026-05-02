using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{ 
    public static GameManager Instance { get; private set; }
 
    public int lives { get; private set; } = 3;
    public int coins { get; private set; } = 0;
    public int score { get; private set; } = 0;
    public int time  { get; set; } = 150;

    private bool levelComplete  = false;
    private static bool isFirstLaunch = true;
 
    private GameObject startPanel;
    private GameObject victoryPanel;
    private GameObject gameOverPanel;
 
    private TMP_InputField nameInput;
 
    private string playerName = "";
 
    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayerPrefs.DeleteAll();
        Application.targetFrameRate = 60;
        FindPanels();

        if (isFirstLaunch)
            Time.timeScale = 0f;    
        else
        {
            Time.timeScale = 1f;
            StartGameLogic();
        }
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        levelComplete = false;
        FindPanels();

        if (isFirstLaunch)
            Time.timeScale = 0f;
        else
        {
            Time.timeScale = 1f;
            StartGameLogic();
        }
    }
 
    private void FindPanels()
    {
        victoryPanel  = null;
        gameOverPanel = null;
        startPanel    = null;
        nameInput     = null;
 
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in allObjects)
        { 
            if (go.scene.name == null || go.scene.name == "") continue;
            if (go.scene.name == "DontDestroyOnLoad") continue;

            switch (go.name)
            {
                case "VictoryPanel":
                    victoryPanel = go;
                    break;
                case "GameOverPanel":
                    gameOverPanel = go;
                    break;
                case "StartPanel":
                    startPanel = go;
                    nameInput  = go.GetComponentInChildren<TMP_InputField>(true);
                    if (nameInput != null) nameInput.characterLimit = 5;
                    break;
            }
        } 
        if (victoryPanel  != null) {
            BindButton(victoryPanel,  "ExitButton",  GoToStart);   
            BindButton(victoryPanel,  "Restart",     RestartGame);
        }
        if (gameOverPanel != null) {
            BindButton(gameOverPanel, "ExitButton",  GoToStart);    
            BindButton(gameOverPanel, "Restart",     RestartGame);
        }
        if (startPanel    != null) {
            startPanel.SetActive(isFirstLaunch);
            BindButton(startPanel,    "StartClickButton", StartGame);
            BindButton(startPanel,    "ExitClickButton",  QuitGame);  
        }
        
        if (victoryPanel  == null) Debug.LogWarning("FindPanels: 'VictoryPanel' не знайдено!");
        if (gameOverPanel == null) Debug.LogWarning("FindPanels: 'GameOverPanel' не знайдено!");
        if (startPanel    == null) Debug.LogWarning("FindPanels: 'StartPanel' не знайдено!");
        if (nameInput     == null) Debug.LogWarning("FindPanels: TMP_InputField не знайдено в StartPanel!");
    }
 
    private void BindButton(GameObject panel, string namePart, UnityEngine.Events.UnityAction action)
    {
        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
             if (btn.gameObject.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                 btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
            }
        }
     }
 
    private void StartGameLogic()
    {
        CancelInvoke(nameof(TickTime));
        InvokeRepeating(nameof(TickTime), 1f, 1f);
    }

    private void TickTime()
    {
        if (levelComplete) return;

        if (time > 0) time--;
        else ResetLevel();
    }
 
    public void AddScore(int amount) => score += amount;

    public void AddCoin()  => coins++;
    public void AddLife()  => lives++;
 
    public void StartWinSequence()
    {
        levelComplete = true;
        CancelInvoke(nameof(TickTime));
    }
 
    public void ShowVictoryUI()
    {
        if (MusicManager.Instance != null) MusicManager.Instance.PlayVictory();
 
        LeaderboardManager lb = FindAnyObjectByType<LeaderboardManager>();
        if (lb != null)
            lb.SaveScore(playerName, score);
         
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
        
    }
 
    public void WinLevel()
    {
        StartWinSequence();
        ShowVictoryUI();
    }
 
    public void ResetLevel(float delay)
    {
        if (levelComplete) return;
        CancelInvoke(nameof(TickTime));
        StartCoroutine(ResetAfterDelay(delay));
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetLevel();
    }

    public void ResetLevel()
    {
        if (levelComplete) return;

        lives--;

        if (lives > 0)
        { 
            coins = 0;
            score = 0;
            time  = 150;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        { 
            ShowGameOver();
            coins = 0;
            score = 0;
            time  = 150;
        }
    }

    private void ShowGameOver()
    {
        CancelInvoke(nameof(TickTime));

        if (MusicManager.Instance != null) MusicManager.Instance.PlayDeath();
 
        LeaderboardManager lb = FindAnyObjectByType<LeaderboardManager>();
        if (lb != null)
            lb.SaveScore(playerName, score);
         

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
         
    }
 
    private void ResetStats()
    {
        levelComplete = false;
        lives = 3;
        coins = 0;
        score = 0;
        time  = 150;
    }

    public void StartGame()
    { 
        playerName = (nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text))
            ? nameInput.text.Trim()
            : "Mario";

        ResetStats();           
        isFirstLaunch = false;

        if (startPanel != null)
            startPanel.SetActive(false);

        Time.timeScale = 1f;
        StartGameLogic();
    }

    public void RestartGame()
    {
        ResetStats();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
 
    public void NewGame() => RestartGame();

     public void GoToStart()
    {
        CancelInvoke(nameof(TickTime));
        ResetStats();
        isFirstLaunch = true;
        Time.timeScale = 0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

     public void QuitGame()
    {
        Debug.Log("Вихід з гри...");
        Application.Quit();
    }
    public void TriggerPlayerJump() {
    PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
    if (player != null) player.OnJumpButtonPressed();
}
}